using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Logic.Loaders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace DataProcessingApp.Tests;

/// <summary>
/// Data invariants for the committed JSONFiles/ data, encoding the checks that
/// caught the 2026 data bugs: row-count grids, the J/K adjustment-factor
/// identity, no " " placeholder strings, and factor sanity/monotonicity.
/// These tests read the real data files from the repository.
/// </summary>
public class DataInvariantTests
{
    private const string RepoRootRelativeJsonFiles = "JSONFiles";

    // IRS publishes factors rounded to 4 decimals; the identity J = K*(1+i)^(1/m)
    // therefore holds only to within ~1.1e-4 (observed max 8.5e-5).
    private const double AdjustmentIdentityTolerance = 1.5e-4;

    private static readonly Dictionary<string, double> PaymentsPerYear = new()
    {
        { "Annual", 1 },
        { "Semiannual", 2 },
        { "Quarterly", 4 },
        { "Monthly", 12 },
        { "Weekly", 52 }
    };

    private static readonly string[] Series = { "90CM", "2000CM", "2010CM" };

    // each IRS era publishes its own section 7520 rate grid: the 90CM era
    // tables start at 2.2%, the 2000CM/2010CM era (incl. Table Z) at 0.2%;
    // two-life adjusted payout rates follow the same split (2.2..6.0 vs 0.2..4.0)
    private static readonly List<double> ModernRateGrid = RateGrid(0.2, 20.0);

    private static readonly List<double> ModernPayoutGrid = RateGrid(0.2, 4.0);

    private static readonly Dictionary<string, List<double>> SeriesRateGrids = new()
    {
        { "90CM", RateGrid(2.2, 22.0) },
        { "2000CM", ModernRateGrid },
        { "2010CM", ModernRateGrid }
    };

    private static readonly Dictionary<string, List<double>> SeriesPayoutGrids = new()
    {
        { "90CM", RateGrid(2.2, 6.0) },
        { "2000CM", ModernPayoutGrid },
        { "2010CM", ModernPayoutGrid }
    };

    // ------------------------------------------------------------------
    // loading (cached once per test run)
    // ------------------------------------------------------------------

    private static readonly Lazy<List<MortalityTableRow>> mortalityRows =
        new(() => LoadJson<MortalityTableRow>("MortalityTable.json"));

    private static readonly Lazy<List<TableJRow>> tableJRows =
        new(() => LoadJson<TableJRow>("TableJ.json"));

    private static readonly Lazy<List<TableKRow>> tableKRows =
        new(() => LoadJson<TableKRow>("TableK.json"));

    private static readonly Lazy<List<TableBRow>> tableBRows =
        new(() => LoadJson<TableBRow>("TableB.json"));

    private static readonly Lazy<List<TableDRow>> tableDRows =
        new(() => LoadJson<TableDRow>("TableD.json"));

    private static readonly Lazy<List<TableFRow>> tableFRows =
        new(() => LoadJson<TableFRow>("TableF.json"));

    private static readonly Lazy<Dictionary<string, List<TableSRow>>> tableSRows =
        new(() => Series.ToDictionary(s => s, s => LoadSeriesJson<TableSRow>(s, $"TableS-{s}-processed.json")));

    private static readonly Lazy<Dictionary<string, List<TableCRow>>> tableCRows =
        new(() => Series.ToDictionary(s => s, s => LoadSeriesJson<TableCRow>(s, $"TableC-{s}-processed.json")));

    private static readonly Lazy<Dictionary<string, List<TableHRow>>> tableHRows =
        new(() => Series.ToDictionary(s => s, s => LoadSeriesJson<TableHRow>(s, $"TableH-{s}-processed.json")));

    private static readonly Lazy<Dictionary<string, List<TableU1Row>>> tableU1Rows =
        new(() => Series.ToDictionary(s => s, s => LoadSeriesJson<TableU1Row>(s, $"TableU1-{s}-processed.json")));

    // Table Z exists for the 2000CM and 2010CM series (not 90CM)
    private static readonly Lazy<Dictionary<string, List<TableZRow>>> tableZRows =
        new(() => new[] { "2000CM", "2010CM" }.ToDictionary(
            s => s, s => LoadSeriesJson<TableZRow>(s, $"TableZ-{s}-processed.json")));

    // two-life tables are committed as 5 parts each; only the first part is
    // fully parsed for shape/monotonicity, the rest are counted streaming
    private static readonly Lazy<Dictionary<string, List<TableU2Row>>> tableU2Part1Rows =
        new(() => Series.ToDictionary(s => s, s => LoadSeriesJson<TableU2Row>(s, $"TableU(2)-p1-{s}-processed.json")));

    private static readonly Lazy<Dictionary<string, List<TableR2Row>>> tableR2Part1Rows =
        new(() => Series.ToDictionary(s => s, s => LoadSeriesJson<TableR2Row>(s, $"TableR(2)-p1-{s}-processed.json")));

    // ------------------------------------------------------------------
    // placeholder strings
    // ------------------------------------------------------------------

    [Fact]
    public void JsonFiles_ContainNoSpacePlaceholderStrings()
    {
        var offenders = Directory
            .EnumerateFiles(JsonFilesDirectory(), "*.json", SearchOption.AllDirectories)
            .Where(f => File.ReadAllText(f).Contains("\" \""))
            .Select(Path.GetFileName)
            .ToList();

        Assert.True(offenders.Count == 0, "Placeholder \" \" values found in: " + string.Join(", ", offenders));
    }

    // ------------------------------------------------------------------
    // row-count grids
    // ------------------------------------------------------------------

    [Fact]
    public void MortalityTable_CoversFourYearsWithAges0To110()
    {
        var rows = mortalityRows.Value;
        Assert.Equal(444, rows.Count);

        var years = rows.Select(r => r.Year).Distinct().OrderBy(y => y).ToList();
        Assert.Equal(new[] { 1980, 1990, 2000, 2010 }, years);

        foreach (var year in years)
        {
            var ages = rows.Where(r => r.Year == year).Select(r => r.Age).OrderBy(a => a).ToList();
            Assert.Equal(Enumerable.Range(0, 111), ages);
        }
    }

    [Fact]
    public void RootTables_MatchTheirPublishedGrids()
    {
        // Table J and K: 100 rates (0.2 .. 20.0, step 0.2) x 5 frequencies
        var rates = RateGrid(0.2, 20.0);
        foreach (var frequency in PaymentsPerYear.Keys)
        {
            foreach (var rate in rates)
            {
                Assert.Single(tableJRows.Value, r => r.InterestRate == rate && r.Frequency == frequency);
                Assert.Single(tableKRows.Value, r => r.InterestRate == rate && r.Frequency == frequency);
            }
        }
        Assert.Equal(500, tableJRows.Value.Count);
        Assert.Equal(500, tableKRows.Value.Count);

        // Table F: 100 rates x (Annual months 0..12, Semiannual 0..6, Quarterly 0..3, Monthly 0..1)
        var monthLimits = new Dictionary<string, int>
        {
            { "Annual", 12 },
            { "Semiannual", 6 },
            { "Quarterly", 3 },
            { "Monthly", 1 }
        };
        Assert.Equal(2600, tableFRows.Value.Count);
        foreach (var rate in rates)
        {
            foreach (var frequency in monthLimits)
            {
                for (var months = 0; months <= frequency.Value; months++)
                {
                    Assert.Single(tableFRows.Value, r => r.InterestRate == rate && r.Frequency == frequency.Key && r.Months == months);
                }
            }
        }

        // Table B: 100 rates x 60 terms (1..60)
        Assert.Equal(6000, tableBRows.Value.Count);
        foreach (var rate in rates)
        {
            for (var years = 1; years <= 60; years++)
            {
                Assert.Single(tableBRows.Value, r => r.Rate == rate && r.Years == years);
            }
        }

        // Table D: 100 payout rates x 20 terms (1..20)
        Assert.Equal(2000, tableDRows.Value.Count);
        foreach (var rate in rates)
        {
            for (var years = 1; years <= 20; years++)
            {
                Assert.Single(tableDRows.Value, r => r.PayoutRate == rate && r.Years == years);
            }
        }
    }

    [Fact]
    public void SeriesTables_Cover100RatesBy110Ages()
    {
        // 100 rates x 110 ages (0..109) = 11000 rows; the rate grid differs by era
        var ages = Enumerable.Range(0, 110).ToList();

        foreach (var series in Series)
        {
            var rates = SeriesRateGrids[series];

            Assert.Equal(11000, tableSRows.Value[series].Count);
            AssertCrossProduct(tableSRows.Value[series], rates, ages, r => r.InterestRate, r => r.Age, $"TableS {series}");

            Assert.Equal(11000, tableCRows.Value[series].Count);
            AssertCrossProduct(tableCRows.Value[series], rates, ages, r => r.Rate, r => r.Age, $"TableC {series}");

            Assert.Equal(11000, tableHRows.Value[series].Count);
            AssertCrossProduct(tableHRows.Value[series], rates, ages, r => r.InterestRate, r => r.Age, $"TableH {series}");

            Assert.Equal(11000, tableU1Rows.Value[series].Count);
            AssertCrossProduct(tableU1Rows.Value[series], rates, ages, r => r.AdjustedPayoutRate, r => r.Age, $"TableU(1) {series}");

            if (series != "90CM")
            {
                Assert.Equal(11000, tableZRows.Value[series].Count);
                AssertCrossProduct(tableZRows.Value[series], rates, ages, r => r.InterestRate, r => r.Age, $"TableZ {series}");
            }
        }
    }

    [Fact]
    public void TwoLifeParts_Cover6105AgePairsBy20PayoutRates()
    {
        // 110 x 110 / 2 lower-triangular (age2 <= age1, both 0..109) = 6105 pairs
        // x 20 payout rates = 122100 rows per part; payout grids differ by era
        foreach (var series in Series)
        {
            var payouts = SeriesPayoutGrids[series];
            foreach (var table in new[] { "TableR(2)", "TableU(2)" })
            {
                var parts = Enumerable.Range(1, 5)
                    .Select(p => SeriesPath(series, $"{table}-p{p}-{series}-processed.json"))
                    .ToList();
                Assert.Equal(5, parts.Count(File.Exists));

                foreach (var part in parts)
                {
                    Assert.Equal(122100, CountJsonArrayElements(part));
                }
            }

            // full shape check on the first part only (parsing all parts is slow)
            AssertPart1Shape(tableU2Part1Rows.Value[series], payouts, r => r.Age1, r => r.Age2, r => r.AdjustedPayoutRate, $"TableU(2) {series}");
            AssertPart1Shape(tableR2Part1Rows.Value[series], payouts, r => r.Age1, r => r.Age2, r => r.AdjustedPayoutRate, $"TableR(2) {series}");
        }
    }

    private static void AssertPart1Shape<TRow>(
        List<TRow> rows, List<double> payouts,
        Func<TRow, int> age1, Func<TRow, int> age2, Func<TRow, double> payout,
        string context)
    {
        Assert.Equal(122100, rows.Count);
        Assert.Equal(6105, rows.Select(r => (age1(r), age2(r))).Distinct().Count());

        foreach (var p in payouts)
        {
            var pairCount = rows.Where(r => payout(r) == p)
                .Select(r => (age1(r), age2(r))).Distinct().Count();
            Assert.Equal(6105, pairCount);
        }

        var badPair = rows.FirstOrDefault(r => age1(r) < age2(r) || age1(r) < 0 || age1(r) > 109);
        Assert.True(badPair == null, $"{context}: age pair outside 0 <= age2 <= age1 <= 109");
    }

    // ------------------------------------------------------------------
    // J/K adjustment-factor identity
    // ------------------------------------------------------------------

    public static IEnumerable<object[]> AdjustmentFactorPairs()
    {
        return tableKRows.Value.Select(k => new object[] { k.InterestRate, k.Frequency, k.AdjustmentFactor });
    }

    [Theory]
    [MemberData(nameof(AdjustmentFactorPairs))]
    public void TableJ_MatchesTableKIdentity(double rate, string frequency, double kFactor)
    {
        // J = K * (1+i)^(1/m): J (payments in advance) equals K (payments at
        // the end of the period) grossed up by one payment interval
        var jFactor = tableJRows.Value.Single(r => r.InterestRate == rate && r.Frequency == frequency).AdjustmentFactor;
        var paymentsPerYear = PaymentsPerYear[frequency];

        var expected = kFactor * Math.Pow(1 + rate / 100.0, 1.0 / paymentsPerYear);

        Assert.True(
            Math.Abs(jFactor - expected) <= AdjustmentIdentityTolerance,
            $"{frequency} @ {rate}%: J={jFactor} deviates from K*(1+i)^(1/m)={expected} by {Math.Abs(jFactor - expected)}");
    }

    // ------------------------------------------------------------------
    // factor sanity / monotonicity
    // ------------------------------------------------------------------

    [Fact]
    public void MortalityTable_LxStartsAt100000AndDecreasesWithAge()
    {
        foreach (var year in mortalityRows.Value.Select(r => r.Year).Distinct())
        {
            var curve = mortalityRows.Value.Where(r => r.Year == year).OrderBy(r => r.Age).ToList();

            Assert.Equal(100000, curve[0].Lx);
            for (var i = 1; i < curve.Count; i++)
            {
                Assert.True(curve[i].Lx < curve[i - 1].Lx,
                    $"Mortality {year}: lx increases at age {curve[i].Age} ({curve[i - 1].Lx} -> {curve[i].Lx})");
            }
        }
    }

    [Fact]
    public void SeriesFactors_MonotonicInAgeAndPositive()
    {
        // infant mortality makes values at the youngest ages non-monotonic
        // (surviving age 1 raises life expectancy); monotonicity is asserted
        // from age 5 where the published tables are clean
        const int monotonicFromAge = 5;

        foreach (var series in Series)
        {
            AssertMonotonic(tableSRows.Value[series], r => $"{r.InterestRate}", r => r.Age, r => r.PvAnnuity, increasing: false, monotonicFromAge, $"TableS {series} pvAnnuity");
            AssertMonotonic(tableSRows.Value[series], r => $"{r.InterestRate}", r => r.Age, r => r.PvLifeEstate, increasing: false, monotonicFromAge, $"TableS {series} pvLifeEstate");
            // pvReminderInterest is the legacy-misspelled pvRemainderInterest key
            AssertMonotonic(tableSRows.Value[series], r => $"{r.InterestRate}", r => r.Age, r => r.PvReminderInterest, increasing: true, monotonicFromAge, $"TableS {series} pvReminderInterest");
            AssertAllPositive(tableSRows.Value[series], r => r.PvAnnuity, $"TableS {series} pvAnnuity");

            AssertMonotonic(tableCRows.Value[series], r => $"{r.Rate}", r => r.Age, r => r.RemainderFactor, increasing: true, monotonicFromAge, $"TableC {series} remainderFactor");
            AssertInRange(tableCRows.Value[series], r => r.RemainderFactor, 0.0, 1.0, $"TableC {series} remainderFactor");

            AssertMonotonic(tableHRows.Value[series], r => $"{r.InterestRate}", r => r.Age, r => r.DFactor, increasing: false, 0, $"TableH {series} dFactor");
            AssertMonotonic(tableHRows.Value[series], r => $"{r.InterestRate}", r => r.Age, r => r.NFactor, increasing: false, 0, $"TableH {series} nFactor");
            AssertAllPositive(tableHRows.Value[series], r => r.DFactor, $"TableH {series} dFactor");

            AssertMonotonic(tableU1Rows.Value[series], r => $"{r.AdjustedPayoutRate}", r => r.Age, r => r.RemainderFactor, increasing: true, monotonicFromAge, $"TableU(1) {series} remainderFactor");
            AssertInRange(tableU1Rows.Value[series], r => r.RemainderFactor, 0.0, 1.0, $"TableU(1) {series} remainderFactor");

            if (series != "90CM")
            {
                AssertMonotonic(tableZRows.Value[series], r => $"{r.InterestRate}", r => r.Age, r => r.DFactor, increasing: false, 0, $"TableZ {series} dFactor");
                AssertMonotonic(tableZRows.Value[series], r => $"{r.InterestRate}", r => r.Age, r => r.NFactor, increasing: false, 0, $"TableZ {series} nFactor");
                AssertAllPositive(tableZRows.Value[series], r => r.DFactor, $"TableZ {series} dFactor");
            }
        }
    }

    [Fact]
    public void RootFactors_MonotonicInTermAndPositive()
    {
        // annuity and income interest grow with the term, remainder value decays
        AssertMonotonic(tableBRows.Value, r => $"{r.Rate}", r => r.Years, r => r.PvAnnuity, increasing: true, 1, "TableB pvAnnuity");
        AssertMonotonic(tableBRows.Value, r => $"{r.Rate}", r => r.Years, r => r.PvIncomeInterest, increasing: true, 1, "TableB pvIncomeInterest");
        AssertMonotonic(tableBRows.Value, r => $"{r.Rate}", r => r.Years, r => r.PvRemainderInterest, increasing: false, 1, "TableB pvRemainderInterest");
        AssertAllPositive(tableBRows.Value, r => r.PvAnnuity, "TableB pvAnnuity");

        AssertMonotonic(tableDRows.Value, r => $"{r.PayoutRate}", r => r.Years, r => r.RemainderInterest, increasing: false, 1, "TableD remainderInterest");
        AssertAllPositive(tableDRows.Value, r => r.RemainderInterest, "TableD remainderInterest");
    }

    [Fact]
    public void TwoLifeFactors_MonotonicInAgesAndPayout()
    {
        foreach (var series in Series)
        {
            // a higher payout rate depletes the corpus faster, shrinking the remainder
            AssertMonotonic(tableU2Part1Rows.Value[series], r => $"{r.AdjustedPayoutRate}/{r.Age2}", r => r.Age1, r => r.RemainderFactor, increasing: true, 0, $"TableU(2) {series} vs age1");
            AssertMonotonic(tableU2Part1Rows.Value[series], r => $"{r.AdjustedPayoutRate}/{r.Age1}", r => r.Age2, r => r.RemainderFactor, increasing: true, 1, $"TableU(2) {series} vs age2");
            AssertMonotonic(tableU2Part1Rows.Value[series], r => $"{r.Age1}/{r.Age2}", r => r.AdjustedPayoutRate, r => r.RemainderFactor, increasing: false, 0, $"TableU(2) {series} vs payout");
            AssertInRange(tableU2Part1Rows.Value[series], r => r.RemainderFactor, 0.0, 1.0, $"TableU(2) {series} remainderFactor");

            AssertMonotonic(tableR2Part1Rows.Value[series], r => $"{r.AdjustedPayoutRate}/{r.Age2}", r => r.Age1, r => r.RemainderFactor, increasing: true, 0, $"TableR(2) {series} vs age1");
            AssertMonotonic(tableR2Part1Rows.Value[series], r => $"{r.AdjustedPayoutRate}/{r.Age1}", r => r.Age2, r => r.RemainderFactor, increasing: true, 1, $"TableR(2) {series} vs age2");
            AssertMonotonic(tableR2Part1Rows.Value[series], r => $"{r.Age1}/{r.Age2}", r => r.AdjustedPayoutRate, r => r.RemainderFactor, increasing: false, 0, $"TableR(2) {series} vs payout");
            AssertInRange(tableR2Part1Rows.Value[series], r => r.RemainderFactor, 0.0, 1.0, $"TableR(2) {series} remainderFactor");
        }
    }

    // ------------------------------------------------------------------
    // helpers
    // ------------------------------------------------------------------

    private static List<double> RateGrid(double from, double to)
    {
        var rates = new List<double>();
        for (var rate = from; rate <= to + 0.005; rate += 0.2)
        {
            rates.Add(Math.Round(rate, 1));
        }
        return rates;
    }

    private static void AssertCrossProduct<TRow>(
        List<TRow> rows, List<double> rates, List<int> ages,
        Func<TRow, double> rate, Func<TRow, int> age, string context)
    {
        var seen = new HashSet<(double, int)>(rows.Select(r => (rate(r), age(r))));
        Assert.Equal(rates.Count * ages.Count, seen.Count);

        foreach (var r in rates)
        {
            foreach (var a in ages)
            {
                Assert.True(seen.Contains((r, a)), $"{context}: missing cell rate={r} age={a}");
            }
        }
    }

    private static void AssertMonotonic<TRow>(
        IEnumerable<TRow> rows, Func<TRow, string> groupKey, Func<TRow, double> orderBy, Func<TRow, double> value,
        bool increasing, double minOrderBy, string context)
    {
        var violation = (string)null;

        foreach (var group in rows.GroupBy(groupKey))
        {
            var ordered = group.Where(r => orderBy(r) >= minOrderBy).OrderBy(orderBy).Select(value).ToList();
            for (var i = 1; i < ordered.Count && violation == null; i++)
            {
                var previous = ordered[i - 1];
                var current = ordered[i];
                if (increasing ? current < previous : current > previous)
                {
                    violation = $"{context}, group '{group.Key}': value goes {(increasing ? "down" : "up")} from {previous} to {current}";
                }
            }
        }

        Assert.True(violation == null, violation);
    }

    private static void AssertAllPositive<TRow>(IEnumerable<TRow> rows, Func<TRow, double> value, string context)
    {
        var offender = rows.Select(value).Cast<double?>().FirstOrDefault(v => v <= 0);
        Assert.True(offender == null, $"{context}: non-positive factor {offender}");
    }

    private static void AssertInRange<TRow>(IEnumerable<TRow> rows, Func<TRow, double> value, double min, double max, string context)
    {
        foreach (var row in rows)
        {
            var v = value(row);
            Assert.True(v > min && v < max, $"{context}: factor {v} outside ({min}, {max})");
        }
    }

    private static List<TRow> LoadJson<TRow>(string filename) where TRow : new()
    {
        return new TableLoader<TRow>().LoadFromJson(Path.Combine(JsonFilesDirectory(), filename));
    }

    private static List<TRow> LoadSeriesJson<TRow>(string series, string filename) where TRow : new()
    {
        return new TableLoader<TRow>().LoadFromJson(SeriesPath(series, filename));
    }

    private static string SeriesPath(string series, string filename)
    {
        return Path.Combine(JsonFilesDirectory(), series, filename);
    }

    private static int CountJsonArrayElements(string filename)
    {
        using (var streamReader = new StreamReader(filename))
        using (var reader = new JsonTextReader(streamReader))
        {
            var count = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject && reader.Depth == 1)
                {
                    count++;
                }
            }
            return count;
        }
    }

    private static string JsonFilesDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "DataProcessingApp.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return Path.Combine(dir.FullName, RepoRootRelativeJsonFiles);
    }
}