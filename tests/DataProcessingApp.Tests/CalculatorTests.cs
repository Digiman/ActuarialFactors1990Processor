using DataProcessingApp.Calculator;
using DataProcessingApp.Core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace DataProcessingApp.Tests;

/// <summary>
/// Scenario computations of the calculator library against the committed
/// JSONFiles/ data: published identities (income + remainder = 1), the
/// Rev. Proc. 89-21 payout-rate adjustment, frequency-adjustment direction,
/// and the exactness of the underlying table lookups.
/// </summary>
public class CalculatorTests
{
    private const double Tolerance = 1e-9;

    private static readonly FactorData Data = new(JsonFilesDirectory());

    private static readonly ActuarialCalculator Calculator = new(Data);

    // ------------------------------------------------------------------
    // life estate (Table S)
    // ------------------------------------------------------------------

    [Fact]
    public void LifeEstate_MatchesCommittedTableS()
    {
        var result = Calculator.Run("life-estate", "2010CM", Args(("age", "65"), ("rate", "5.2")));

        var remainder = Value(result, "Remainder factor");
        Assert.Equal(0.42012, remainder, 5);
    }

    [Fact]
    public void LifeEstate_IncomePlusRemainderIsOne()
    {
        foreach (var series in FactorData.Series)
        {
            var result = Calculator.Run("life-estate", series, Args(("age", "47"), ("rate", "2.2")));
            var sum = Value(result, "Life estate + remainder");
            Assert.Equal(1.0, sum, 9);
        }
    }

    [Fact]
    public void LifeEstate_OffGridRate_FailsWithActualGrid()
    {
        // 5.3 sits on the 0.1 grid but not on the published 0.2 grid
        var ex = Assert.Throws<ScenarioException>(
            () => Calculator.Run("life-estate", "2010CM", Args(("age", "65"), ("rate", "5.3"))));

        Assert.Contains("0.2-20% in 0.2 steps", ex.Message);
        Assert.Contains("ages 0-109", ex.Message);
    }

    // ------------------------------------------------------------------
    // annuity (Tables S + J/K)
    // ------------------------------------------------------------------

    [Fact]
    public void Annuity_MatchesManualLookup()
    {
        var result = Calculator.Run("annuity", "2010CM",
            Args(("age", "65"), ("rate", "5.2"), ("frequency", "Quarterly"), ("timing", "End")));

        var sRows = Data.TableS("2010CM");
        var sRow = sRows.First(r => r.Age == 65 && GridLookup.Same(r.InterestRate, 5.2));
        var kRow = Data.TableK().First(r => GridLookup.Same(r.InterestRate, 5.2) && r.Frequency == "Quarterly");

        Assert.Equal(sRow.PvAnnuity, Value(result, "Annuity factor (1/yr, annual payments)"), 9);
        Assert.Equal(sRow.PvAnnuity * kRow.AdjustmentFactor, Value(result, "Adjusted annuity factor"), 9);
    }

    [Fact]
    public void Annuity_BeginningFactorExceedsEndFactor()
    {
        var beginning = Calculator.Run("annuity", "90CM",
            Args(("age", "80"), ("rate", "6.2"), ("frequency", "Monthly"), ("timing", "Beginning")));
        var end = Calculator.Run("annuity", "90CM",
            Args(("age", "80"), ("rate", "6.2"), ("frequency", "Monthly"), ("timing", "End")));

        Assert.True(Value(beginning, "Adjusted annuity factor") > Value(end, "Adjusted annuity factor"));
    }

    // ------------------------------------------------------------------
    // unitrust (Tables F + U(1)/U(2))
    // ------------------------------------------------------------------

    [Fact]
    public void Unitrust_AnnualAtValuationDate_KeepsPayoutRate()
    {
        var result = Calculator.Run("unitrust", "2010CM",
            Args(("age", "65"), ("payout", "5.0"), ("frequency", "Annual"), ("months", "0")));

        Assert.Equal(5.0, Value(result, "Adjusted payout rate"), 9);
        Assert.Equal(0.41634, Value(result, "Remainder factor (one life)"), 5);
    }

    [Fact]
    public void Unitrust_FrequentPaymentsRaiseTheAdjustedRate()
    {
        var annual = Calculator.Run("unitrust", "2010CM",
            Args(("age", "70"), ("payout", "8.0"), ("frequency", "Annual"), ("months", "0")));
        var quarterly = Calculator.Run("unitrust", "2010CM",
            Args(("age", "70"), ("payout", "8.0"), ("frequency", "Quarterly"), ("months", "0")));

        Assert.Equal(8.0, Value(annual, "Adjusted payout rate"), 9);
        Assert.True(Value(quarterly, "Adjusted payout rate") > 8.0);
        // higher adjusted payout rate => smaller remainder
        Assert.True(Value(quarterly, "Remainder factor (one life)") < Value(annual, "Remainder factor (one life)"));
    }

    [Fact]
    public void Unitrust_AdjustedRateIsOnThePublishedPayoutGrid()
    {
        var combos = new[] { ("Annual", 0), ("Annual", 6), ("Annual", 12), ("Semiannual", 2), ("Quarterly", 3), ("Monthly", 1) };
        foreach (var (frequency, months) in combos)
        {
            var result = Calculator.Run("unitrust", "2000CM",
                Args(("age", "55"), ("payout", "7.4"), ("frequency", frequency), ("months", months.ToString())));

            var adjusted = Value(result, "Adjusted payout rate");
            Assert.Equal(adjusted, Math.Round(adjusted * 5.0, MidpointRounding.AwayFromZero) / 5.0, 9);
        }
    }

    [Fact]
    public void Unitrust_MonthsBeyondPublishedRange_FailsWithRanges()
    {
        var ex = Assert.Throws<ScenarioException>(
            () => Calculator.Run("unitrust", "2010CM",
                Args(("age", "65"), ("payout", "5.0"), ("frequency", "Monthly"), ("months", "6"))));

        Assert.Contains("Monthly 0-1", ex.Message);
    }

    [Fact]
    public void UnitrustTwoLife_SwapsYoungerAndOlderLife()
    {
        var a = Calculator.Run("unitrust-two-life", "2010CM",
            Args(("age1", "65"), ("age2", "70"), ("payout", "4.2"), ("frequency", "Quarterly"), ("months", "2")));
        var b = Calculator.Run("unitrust-two-life", "2010CM",
            Args(("age1", "70"), ("age2", "65"), ("payout", "4.2"), ("frequency", "Quarterly"), ("months", "2")));

        Assert.Equal(Value(b, "Remainder factor (two lives)"), Value(a, "Remainder factor (two lives)"), 9);
    }

    [Fact]
    public void UnitrustTwoLife_MatchesCommittedPartData()
    {
        var result = Calculator.Run("unitrust-two-life", "90CM",
            Args(("age1", "60"), ("age2", "60"), ("payout", "2.2"), ("frequency", "Annual"), ("months", "0")));

        var expected = Data.TableU2("90CM").First(
            r => r.Age1 == 60 && r.Age2 == 60 && GridLookup.Same(r.AdjustedPayoutRate, 2.2));
        Assert.Equal(expected.RemainderFactor, Value(result, "Remainder factor (two lives)"), 9);
    }

    // ------------------------------------------------------------------
    // annuity trust, two lives (Table R(2))
    // ------------------------------------------------------------------

    [Fact]
    public void AnnuityTrustTwoLife_MatchesCommittedR2()
    {
        var result = Calculator.Run("annuity-trust-two-life", "2000CM",
            Args(("age1", "72"), ("age2", "69"), ("rate", "4.0")));

        var expected = Data.TableR2("2000CM").First(
            r => r.Age1 == 72 && r.Age2 == 69 && GridLookup.Same(r.AdjustedPayoutRate, 4.0));
        Assert.Equal(expected.RemainderFactor, Value(result, "Remainder factor (two lives)"), 9);
    }

    // ------------------------------------------------------------------
    // term certain (Table B) and term unitrust (Table D)
    // ------------------------------------------------------------------

    [Fact]
    public void TermCertain_IncomePlusRemainderIsOne()
    {
        foreach (var years in new[] { 1, 10, 30, 60 })
        {
            var result = Calculator.Run("term-certain", null,
                Args(("years", years.ToString()), ("rate", "3.4")));
            Assert.Equal(1.0, Value(result, "Income + remainder"), 9);
        }
    }

    [Fact]
    public void TermCertain_AnnuityFactorGrowsWithTerm()
    {
        var short5 = Calculator.Run("term-certain", null, Args(("years", "5"), ("rate", "6.0")));
        var long50 = Calculator.Run("term-certain", null, Args(("years", "50"), ("rate", "6.0")));

        Assert.True(Value(long50, "Annuity factor (1/yr for the term)") > Value(short5, "Annuity factor (1/yr for the term)"));
    }

    [Fact]
    public void TermUnitrust_MatchesCommittedTableD()
    {
        var result = Calculator.Run("term-unitrust", null, Args(("years", "12"), ("payout", "6.6")));

        var expected = Data.TableD().First(r => r.Years == 12 && GridLookup.Same(r.PayoutRate, 6.6));
        Assert.Equal(expected.RemainderInterest, Value(result, "Remainder factor (postponed for the term)"), 9);
    }

    // ------------------------------------------------------------------
    // mortality
    // ------------------------------------------------------------------

    [Fact]
    public void Mortality_MatchesCommittedMortalityTable()
    {
        var result = Calculator.Run("mortality", "2010CM", Args(("age", "60")));

        Assert.Equal(88665.95, Value(result, "lx (survivors at age 60, 2010 census)"), 5);
        Assert.Equal(0.8866595, Value(result, "Probability of surviving to this age"), 9);
    }

    // ------------------------------------------------------------------
    // argument and scenario validation
    // ------------------------------------------------------------------

    [Fact]
    public void UnknownScenario_FailsListingScenarios()
    {
        var ex = Assert.Throws<ScenarioException>(() => Calculator.Run("nope", "2010CM", Args()));

        Assert.Contains("nope", ex.Message);
        Assert.Contains("life-estate", ex.Message);
    }

    [Fact]
    public void UnknownSeries_FailsListingSeries()
    {
        Assert.Throws<ScenarioException>(() => Calculator.Run("life-estate", "1980CM", Args(("age", "65"), ("rate", "5.2"))));
    }

    [Fact]
    public void AgeOutOfRange_Fails()
    {
        Assert.Throws<ScenarioException>(
            () => Calculator.Run("life-estate", "2010CM", Args(("age", "110"), ("rate", "5.2"))));
    }

    [Fact]
    public void BadChoice_FailsListingChoices()
    {
        var ex = Assert.Throws<ScenarioException>(
            () => Calculator.Run("annuity", "2010CM", Args(("age", "65"), ("rate", "5.2"), ("frequency", "Daily"))));

        Assert.Contains("Weekly", ex.Message);
    }

    [Fact]
    public void NonNumericRate_Fails()
    {
        Assert.Throws<ScenarioException>(
            () => Calculator.Run("life-estate", "2010CM", Args(("age", "65"), ("rate", "5.2%"))));
    }

    [Fact]
    public void TermCertain_IgnoresSeries()
    {
        var result = Calculator.Run("term-certain", "90CM", Args(("years", "10"), ("rate", "5.2")));

        Assert.Null(result.Series);
        Assert.Equal(0, result.CensusYear);
    }

    // ------------------------------------------------------------------
    // helpers
    // ------------------------------------------------------------------

    private static ScenarioArguments Args(params (string name, string value)[] inputs)
    {
        var values = new Dictionary<string, string>();
        foreach (var (name, value) in inputs)
        {
            values[name] = value;
        }

        return new ScenarioArguments(values);
    }

    private static double Value(ScenarioResult result, string label)
    {
        return result.Values.First(v => v.Label == label).Value;
    }

    private static string JsonFilesDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "DataProcessingApp.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return Path.Combine(dir.FullName, "JSONFiles");
    }
}