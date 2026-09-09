using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DataProcessingApp.Calculator;

/// <summary>
/// Computes actuarial scenarios from the loaded tables. Every scenario is an
/// exact published-grid lookup (or a small published formula over exact
/// lookups); nothing here interpolates or invents factors.
/// </summary>
public sealed class ActuarialCalculator
{
    private readonly FactorData _data;

    /// <summary>Published Table F month ranges per payment frequency.</summary>
    private static readonly Dictionary<string, int> PayoutFrequencyMonths = new()
    {
        { "Annual", 12 },
        { "Semiannual", 6 },
        { "Quarterly", 3 },
        { "Monthly", 1 }
    };

    public ActuarialCalculator(FactorData data)
    {
        _data = data;
    }

    public string DefaultSeries => FilesHelper.Series2010CM;

    /// <summary>
    /// Runs a scenario by name (see <see cref="ScenarioCatalog"/>). The result
    /// carries the table citation and the labeled values to display, so every
    /// consumer (CLI, web API, tests) renders results the same way.
    /// </summary>
    public ScenarioResult Run(string name, string series, ScenarioArguments args)
    {
        var descriptor = ScenarioCatalog.Find(name);
        if (descriptor.SeriesBased)
        {
            series = string.IsNullOrEmpty(series) ? DefaultSeries : series;
            FactorData.ValidateSeries(series);
        }
        else
        {
            series = null;
        }

        var result = new ScenarioResult
        {
            Scenario = descriptor.Name,
            Title = descriptor.Title,
            Purpose = descriptor.Purpose,
            Citation = descriptor.Citation,
            Series = descriptor.SeriesBased ? series : null,
            CensusYear = descriptor.SeriesBased ? _data.CensusYear(series) : 0
        };

        switch (descriptor.Name)
        {
            case "life-estate":
                LifeEstate(result, series, args, descriptor);
                break;
            case "annuity":
                Annuity(result, series, args, descriptor);
                break;
            case "unitrust":
                Unitrust(result, series, args, descriptor);
                break;
            case "unitrust-two-life":
                UnitrustTwoLife(result, series, args, descriptor);
                break;
            case "annuity-trust-two-life":
                AnnuityTrustTwoLife(result, series, args, descriptor);
                break;
            case "term-certain":
                TermCertain(result, args, descriptor);
                break;
            case "term-unitrust":
                TermUnitrust(result, args, descriptor);
                break;
            case "mortality":
                Mortality(result, series, args, descriptor);
                break;
            default:
                throw new ScenarioException($"Scenario '{descriptor.Name}' is registered but not implemented.");
        }

        return result;
    }

    // ------------------------------------------------------------------
    // scenarios
    // ------------------------------------------------------------------

    /// <summary>Table S: life estate (income interest) + remainder = 1 for one life.</summary>
    private void LifeEstate(ScenarioResult result, string series, ScenarioArguments args, ScenarioDescriptor descriptor)
    {
        var age = GetAge(args, descriptor, "age");
        var rate = args.GetRate("rate", descriptor.Inputs.First(i => i.Name == "rate"));

        var rows = _data.TableS(series);
        var grid = GridLookup.DistinctRates(rows, r => r.InterestRate);
        var row = GridLookup.FindRow(
            rows,
            r => r.Age == age && GridLookup.Same(r.InterestRate, rate),
            $"No factor for age {age} at {FormatRate(rate)} (published grid: {GridLookup.GridLabel(grid)}, {GridLookup.AgeGridLabel})",
            $"TableS-{series}");

        result.Add("Annuity factor (1/yr for life)", row.PvAnnuity, "Table S pvAnnuity");
        result.Add("Life estate / income interest factor", row.PvLifeEstate, "Table S pvLifeEstate");
        result.Add("Remainder factor", row.PvReminderInterest, "Table S pvRemainderInterest");
        result.Add("Life estate + remainder", row.PvLifeEstate + row.PvReminderInterest, "always 1");
    }

    /// <summary>Tables S + J/K: annuity factor adjusted for payment frequency.</summary>
    private void Annuity(ScenarioResult result, string series, ScenarioArguments args, ScenarioDescriptor descriptor)
    {
        var age = GetAge(args, descriptor, "age");
        var rate = args.GetRate("rate", descriptor.Inputs.First(i => i.Name == "rate"));
        var frequency = args.GetChoice("frequency", descriptor.Inputs.First(i => i.Name == "frequency"));
        var timing = args.GetChoice("timing", descriptor.Inputs.First(i => i.Name == "timing"));

        var rows = _data.TableS(series);
        var grid = GridLookup.DistinctRates(rows, r => r.InterestRate);
        var sRow = GridLookup.FindRow(
            rows,
            r => r.Age == age && GridLookup.Same(r.InterestRate, rate),
            $"No annuity factor for age {age} at {FormatRate(rate)} (published grid: {GridLookup.GridLabel(grid)}, {GridLookup.AgeGridLabel})",
            $"TableS-{series}");

        double adjustment;
        if (timing == "Beginning")
        {
            adjustment = AdjustmentFactor(_data.TableJ(), "TableJ",
                r => r.InterestRate, r => r.Frequency, r => (int?)null, r => r.AdjustmentFactor,
                rate, frequency, null);
        }
        else
        {
            adjustment = AdjustmentFactor(_data.TableK(), "TableK",
                r => r.InterestRate, r => r.Frequency, r => (int?)null, r => r.AdjustmentFactor,
                rate, frequency, null);
        }

        result.Add("Annuity factor (1/yr, annual payments)", sRow.PvAnnuity, "Table S pvAnnuity");
        result.Add($"{frequency} adjustment factor ({timing.ToLowerInvariant()})", adjustment,
            timing == "Beginning" ? "Table J" : "Table K");
        result.Add("Adjusted annuity factor", sRow.PvAnnuity * adjustment, "value of 1/yr paid at this frequency");
    }

    /// <summary>Tables F + U(1): one-life CRUT remainder with payout-rate adjustment.</summary>
    private void Unitrust(ScenarioResult result, string series, ScenarioArguments args, ScenarioDescriptor descriptor)
    {
        var age = GetAge(args, descriptor, "age");
        var payout = args.GetRate("payout", descriptor.Inputs.First(i => i.Name == "payout"));
        var frequency = args.GetChoice("frequency", descriptor.Inputs.First(i => i.Name == "frequency"));
        var months = GetMonths(args, descriptor);

        var adjusted = AdjustedPayoutRate(payout, frequency, months);
        var rows = _data.TableU1(series);
        var grid = GridLookup.DistinctRates(rows, r => r.AdjustedPayoutRate);
        var row = GridLookup.FindRow(
            rows,
            r => r.Age == age && GridLookup.Same(r.AdjustedPayoutRate, adjusted),
            $"No unitrust remainder for age {age} at adjusted payout rate {FormatRate(adjusted)} (published grid: {GridLookup.GridLabel(grid)}, {GridLookup.AgeGridLabel})",
            $"TableU(1)-{series}");

        result.Add("Payout rate", payout, "as declared by the trust");
        result.Add("Adjusted payout rate", adjusted, "payout / Table F factor, nearest 0.2 step");
        result.Add("Remainder factor (one life)", row.RemainderFactor, "Table U(1)");
    }

    /// <summary>Tables F + U(2): two-life CRUT remainder with payout-rate adjustment.</summary>
    private void UnitrustTwoLife(ScenarioResult result, string series, ScenarioArguments args, ScenarioDescriptor descriptor)
    {
        var age1 = GetAge(args, descriptor, "age1");
        var age2 = GetAge(args, descriptor, "age2");
        if (age2 > age1)
        {
            (age1, age2) = (age2, age1);
        }

        var payout = args.GetRate("payout", descriptor.Inputs.First(i => i.Name == "payout"));
        var frequency = args.GetChoice("frequency", descriptor.Inputs.First(i => i.Name == "frequency"));
        var months = GetMonths(args, descriptor);

        var adjusted = AdjustedPayoutRate(payout, frequency, months);
        var rows = _data.TableU2(series);
        var grid = GridLookup.DistinctRates(rows, r => r.AdjustedPayoutRate);
        var row = GridLookup.FindRow(
            rows,
            r => r.Age1 == age1 && r.Age2 == age2 && GridLookup.Same(r.AdjustedPayoutRate, adjusted),
            $"No two-life unitrust remainder for ages {age1}/{age2} at adjusted payout rate {FormatRate(adjusted)} (published grid: {GridLookup.GridLabel(grid)}, age pairs 0-109 with age2 <= age1)",
            $"TableU(2)-{series}");

        result.Add("Payout rate", payout, "as declared by the trust");
        result.Add("Adjusted payout rate", adjusted, "payout / Table F factor, nearest 0.2 step");
        result.Add("Remainder factor (two lives)", row.RemainderFactor, "Table U(2)");
    }

    /// <summary>Table R(2): two-life annuity remainder at a fixed interest rate.</summary>
    private void AnnuityTrustTwoLife(ScenarioResult result, string series, ScenarioArguments args, ScenarioDescriptor descriptor)
    {
        var age1 = GetAge(args, descriptor, "age1");
        var age2 = GetAge(args, descriptor, "age2");
        if (age2 > age1)
        {
            (age1, age2) = (age2, age1);
        }

        var rate = args.GetRate("rate", descriptor.Inputs.First(i => i.Name == "rate"));
        var rows = _data.TableR2(series);
        var grid = GridLookup.DistinctRates(rows, r => r.AdjustedPayoutRate);
        var row = GridLookup.FindRow(
            rows,
            r => r.Age1 == age1 && r.Age2 == age2 && GridLookup.Same(r.AdjustedPayoutRate, rate),
            $"No two-life annuity remainder for ages {age1}/{age2} at {FormatRate(rate)} (published grid: {GridLookup.GridLabel(grid)}, age pairs 0-109 with age2 <= age1)",
            $"TableR(2)-{series}");

        result.Add("Section 7520 rate", rate);
        result.Add("Remainder factor (two lives)", row.RemainderFactor, "Table R(2)");
    }

    /// <summary>Table B: annuity, income interest and remainder for a term of years.</summary>
    private void TermCertain(ScenarioResult result, ScenarioArguments args, ScenarioDescriptor descriptor)
    {
        var years = GetYears(args, descriptor, 1, 60);
        var rate = args.GetRate("rate", descriptor.Inputs.First(i => i.Name == "rate"));

        var rows = _data.TableB();
        var grid = GridLookup.DistinctRates(rows, r => r.Rate);
        var row = GridLookup.FindRow(
            rows,
            r => (int)Math.Round(r.Years) == years && GridLookup.Same(r.Rate, rate),
            $"No term-certain factor for {years} year(s) at {FormatRate(rate)} (published grid: {GridLookup.GridLabel(grid)}, terms 1-60 years)",
            "TableB");

        result.Add("Annuity factor (1/yr for the term)", row.PvAnnuity, "Table B pvAnnuity");
        result.Add("Income interest factor", row.PvIncomeInterest, "Table B pvIncomeInterest");
        result.Add("Remainder factor", row.PvRemainderInterest, "Table B pvRemainderInterest");
        result.Add("Income + remainder", row.PvIncomeInterest + row.PvRemainderInterest, "always 1");
    }

    /// <summary>Table D: unitrust remainder postponed for a term of years.</summary>
    private void TermUnitrust(ScenarioResult result, ScenarioArguments args, ScenarioDescriptor descriptor)
    {
        var years = GetYears(args, descriptor, 1, 20);
        var payout = args.GetRate("payout", descriptor.Inputs.First(i => i.Name == "payout"));

        var rows = _data.TableD();
        var grid = GridLookup.DistinctRates(rows, r => r.PayoutRate);
        var row = GridLookup.FindRow(
            rows,
            r => r.Years == years && GridLookup.Same(r.PayoutRate, payout),
            $"No term unitrust remainder for {years} year(s) at {FormatRate(payout)} (published grid: {GridLookup.GridLabel(grid)}, terms 1-20 years)",
            "TableD");

        result.Add("Remainder factor (postponed for the term)", row.RemainderInterest, "Table D");
    }

    /// <summary>MortalityTable: lx survivors at an age for the census year of the series.</summary>
    private void Mortality(ScenarioResult result, string series, ScenarioArguments args, ScenarioDescriptor descriptor)
    {
        var age = GridLookup.ValidateRange(args.GetInt("age", descriptor.Inputs.First(i => i.Name == "age")), 0, 110, "age");
        var year = _data.CensusYear(series);
        var row = GridLookup.FindRow(
            _data.Mortality(year),
            r => r.Age == age,
            $"No lx value for age {age} in the {year} census table",
            $"MortalityTable-{year}");

        result.Add($"lx (survivors at age {age}, {year} census)", row.Lx, "out of 100,000 born");
        result.Add("Probability of surviving to this age", row.Lx / 100000.0, "lx / 100000");
    }

    // ------------------------------------------------------------------
    // published formulas over exact lookups
    // ------------------------------------------------------------------

    /// <summary>
    /// Rev. Proc. 89-21: the adjusted payout rate is the payout rate divided by
    /// the Table F factor, determined to the nearest 0.2 of one percent.
    /// </summary>
    private double AdjustedPayoutRate(double payout, string frequency, int months)
    {
        if (!PayoutFrequencyMonths.TryGetValue(frequency, out var maxMonths))
        {
            throw new ScenarioException(
                $"Table F publishes no factors for frequency '{frequency}'; published frequencies: {string.Join(", ", PayoutFrequencyMonths.Keys)}.");
        }

        if (months < 0 || months > maxMonths)
        {
            throw new ScenarioException(
                $"Table F publishes no {frequency} factors at {months} month(s) delay; published ranges: " +
                string.Join(", ", PayoutFrequencyMonths.OrderBy(p => p.Value).Select(p => $"{p.Key} 0-{p.Value}")) + ".");
        }

        var factor = AdjustmentFactor(_data.TableF(), "TableF",
            r => r.InterestRate, r => r.Frequency, r => (int?)r.Months, r => r.AdjustmentFactor,
            payout, frequency, months);

        var adjusted = Math.Round(payout / factor * 5.0, MidpointRounding.AwayFromZero) / 5.0;
        return adjusted;
    }

    /// <summary>
    /// Exact adjustment-factor lookup: Table F needs (rate, frequency, months),
    /// Tables J/K need (rate, frequency). monthsAccessor returns null for J/K.
    /// </summary>
    private double AdjustmentFactor<TRow>(
        IReadOnlyList<TRow> rows, string tableLabel,
        Func<TRow, double> rateOf, Func<TRow, string> frequencyOf, Func<TRow, int?> monthsOf, Func<TRow, double> factorOf,
        double rate, string frequency, int? months)
    {
        var grid = GridLookup.DistinctRates(rows, rateOf);
        var row = GridLookup.FindRow(
            rows,
            r =>
            {
                if (!GridLookup.Same(rateOf(r), rate) ||
                    !string.Equals(frequencyOf(r), frequency, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                var rowMonths = monthsOf(r);
                return !months.HasValue || (rowMonths.HasValue && rowMonths.Value == months.Value);
            },
            $"No adjustment factor for {FormatRate(rate)}, {frequency}{(months.HasValue ? $", {months} month(s)" : "")} (published grid: {GridLookup.GridLabel(grid)})",
            tableLabel);

        return factorOf(row);
    }

    // ------------------------------------------------------------------
    // input helpers
    // ------------------------------------------------------------------

    private int GetAge(ScenarioArguments args, ScenarioDescriptor descriptor, string name)
    {
        var value = args.GetInt(name, descriptor.Inputs.First(i => i.Name == name));
        return GridLookup.ValidateRange(value, 0, 109, name);
    }

    private int GetYears(ScenarioArguments args, ScenarioDescriptor descriptor, int min, int max)
    {
        var value = args.GetInt("years", descriptor.Inputs.First(i => i.Name == "years"));
        return GridLookup.ValidateRange(value, min, max, "years");
    }

    private int GetMonths(ScenarioArguments args, ScenarioDescriptor descriptor)
    {
        var value = args.GetInt("months", descriptor.Inputs.First(i => i.Name == "months"));
        return GridLookup.ValidateRange(value, 0, 12, "months");
    }

    private static string FormatRate(double rate)
    {
        return rate.ToString("0.0", CultureInfo.InvariantCulture) + "%";
    }
}