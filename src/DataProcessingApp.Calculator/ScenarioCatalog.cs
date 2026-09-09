using System.Collections.Generic;
using System.Linq;

namespace DataProcessingApp.Calculator;

public sealed class ScenarioInput
{
    public string Name { get; set; }
    public string Label { get; set; }
    /// <summary>int | rate | choice</summary>
    public string Kind { get; set; }
    public string Default { get; set; }
    public int MinInt { get; set; }
    public int MaxInt { get; set; }
    public double MinRate { get; set; }
    public double MaxRate { get; set; }
    /// <summary>Rate inputs round to the 0.2% grid and follow the series' rate grid (90CM starts at 2.2%).</summary>
    public bool SeriesGrid { get; set; }
    public IReadOnlyList<string> Choices { get; set; }
    public string Help { get; set; }
}

public sealed class ScenarioDescriptor
{
    public string Name { get; set; }
    public string Title { get; set; }
    public string Purpose { get; set; }
    public string Citation { get; set; }
    /// <summary>Grouping used by the web UI sidebar (Single life, Charitable trusts, Terms, Reference).</summary>
    public string Category { get; set; }
    /// <summary>Whether the scenario depends on the census series (90CM / 2000CM / 2010CM).</summary>
    public bool SeriesBased { get; set; }
    public IReadOnlyList<ScenarioInput> Inputs { get; set; }
}

/// <summary>
/// Metadata for every scenario the calculator can compute: the single source
/// of truth for CLI validation, the web UI forms and the API contract.
/// </summary>
public static class ScenarioCatalog
{
    public static readonly string[] RateFrequencies =
    {
        "Annual", "Semiannual", "Quarterly", "Monthly", "Weekly"
    };

    public static readonly string[] PayoutFrequencies =
    {
        "Annual", "Semiannual", "Quarterly", "Monthly"
    };

    public static readonly string[] Timings =
    {
        "Beginning", "End"
    };

    private static readonly List<ScenarioDescriptor> AllScenarios = new()
    {
        new ScenarioDescriptor
        {
            Name = "life-estate",
            Title = "Life estate & remainder (single life)",
            Purpose = "Value an income interest (life estate) and the remainder behind it for one life: the two factors always add up to 1.",
            Citation = "IRS Pub 1457, Table S",
            Category = "Single life",
            SeriesBased = true,
            Inputs = new List<ScenarioInput>
            {
                IntInput("age", "Age", 0, 109, 65, "Age of the measuring life (0-109)."),
                SeriesRateInput("rate", "Section 7520 rate", 5.2, "Section 7520 interest rate (%).")
            }
        },
        new ScenarioDescriptor
        {
            Name = "annuity",
            Title = "Life annuity adjusted for payment frequency",
            Purpose = "Annuity factor for 1 per year for a single life, adjusted for payments made more often than annually (J = beginning of each interval, K = end).",
            Citation = "IRS Pub 1457, Tables S, J and K",
            Category = "Single life",
            SeriesBased = true,
            Inputs = new List<ScenarioInput>
            {
                IntInput("age", "Age", 0, 109, 65, "Age of the measuring life (0-109)."),
                SeriesRateInput("rate", "Section 7520 rate", 5.2, "Section 7520 interest rate (%)."),
                ChoiceInput("frequency", "Payment frequency", RateFrequencies, "Monthly", "How often the annuity pays."),
                ChoiceInput("timing", "Payment timing", Timings, "End", "Beginning of each interval (Table J) or end (Table K).")
            }
        },
        new ScenarioDescriptor
        {
            Name = "unitrust",
            Title = "One-life unitrust remainder (CRUT)",
            Purpose = "Charitable remainder unitrust remainder factor for one life, with the payout rate adjusted for intra-year payment frequency and delay (Table F).",
            Citation = "IRS Pub 1458, Tables F and U(1)",
            Category = "Charitable trusts",
            SeriesBased = true,
            Inputs = new List<ScenarioInput>
            {
                IntInput("age", "Age", 0, 109, 65, "Age of the measuring life (0-109)."),
                SeriesPayoutInput("payout", "Payout rate", 5.0, "Unitrust payout rate (%)."),
                ChoiceInput("frequency", "Payment frequency", PayoutFrequencies, "Quarterly", "How often the trust pays."),
                ChoiceInput("months", "Months to first payout", null, "0", "Delay in months from the annual valuation date to the first payout (Annual 0-12, Semiannual 0-6, Quarterly 0-3, Monthly 0-1).")
            }
        },
        new ScenarioDescriptor
        {
            Name = "unitrust-two-life",
            Title = "Two-life unitrust remainder (CRUT)",
            Purpose = "Charitable remainder unitrust remainder factor for two lives, with the payout rate adjusted for payment frequency and delay (Table F).",
            Citation = "IRS Pub 1458, Tables F and U(2)",
            Category = "Charitable trusts",
            SeriesBased = true,
            Inputs = new List<ScenarioInput>
            {
                IntInput("age1", "Age of older life", 0, 109, 70, "Older measuring life (0-109); swapped with age2 if younger."),
                IntInput("age2", "Age of younger life", 0, 109, 65, "Younger measuring life (0-109)."),
                SeriesPayoutInput("payout", "Payout rate", 4.0, "Unitrust payout rate (%)."),
                ChoiceInput("frequency", "Payment frequency", PayoutFrequencies, "Annual", "How often the trust pays."),
                ChoiceInput("months", "Months to first payout", null, "0", "Delay in months from the annual valuation date to the first payout (Annual 0-12, Semiannual 0-6, Quarterly 0-3, Monthly 0-1).")
            }
        },
        new ScenarioDescriptor
        {
            Name = "annuity-trust-two-life",
            Title = "Two-life annuity remainder (CRAT)",
            Purpose = "Remainder factor behind an annuity trust for two lives at a fixed interest rate (no payout adjustment).",
            Citation = "IRS Pub 1457, Table R(2)",
            Category = "Charitable trusts",
            SeriesBased = true,
            Inputs = new List<ScenarioInput>
            {
                IntInput("age1", "Age of older life", 0, 109, 70, "Older measuring life (0-109); swapped with age2 if younger."),
                IntInput("age2", "Age of younger life", 0, 109, 65, "Younger measuring life (0-109)."),
                SeriesRateInput("rate", "Section 7520 rate", 5.2, "Section 7520 interest rate (%).")
            }
        },
        new ScenarioDescriptor
        {
            Name = "term-certain",
            Title = "Term certain (annuity, income & remainder)",
            Purpose = "Annuity, income interest and remainder factors for a term of years (no mortality basis; one edition serves every series).",
            Citation = "IRS Pub 1457, Table B",
            Category = "Terms",
            SeriesBased = false,
            Inputs = new List<ScenarioInput>
            {
                IntInput("years", "Term (years)", 1, 60, 10, "Term of years (1-60)."),
                RootRateInput("rate", "Section 7520 rate", 5.2, "Section 7520 interest rate (%).")
            }
        },
        new ScenarioDescriptor
        {
            Name = "term-unitrust",
            Title = "Term unitrust remainder",
            Purpose = "Unitrust remainder factor postponed for a term of years (no mortality basis; one edition serves every series).",
            Citation = "IRS Pub 1458, Table D",
            Category = "Terms",
            SeriesBased = false,
            Inputs = new List<ScenarioInput>
            {
                IntInput("years", "Term (years)", 1, 20, 10, "Term of years (1-20)."),
                RootRateInput("payout", "Payout rate", 5.0, "Unitrust payout rate (%).")
            }
        },
        new ScenarioDescriptor
        {
            Name = "mortality",
            Title = "Mortality (lx survivors)",
            Purpose = "Number of survivors (lx) at an age out of 100,000 born, for the census year behind the series.",
            Citation = "IRS Pub 1459, mortality tables (1980 / 1990 / 2000 / 2010 census)",
            Category = "Reference",
            SeriesBased = true,
            Inputs = new List<ScenarioInput>
            {
                IntInput("age", "Age", 0, 110, 65, "Age (0-110).")
            }
        }
    };

    public static IReadOnlyList<ScenarioDescriptor> All => AllScenarios;

    public static ScenarioDescriptor Find(string name)
    {
        var descriptor = AllScenarios.FirstOrDefault(s => s.Name == (name ?? "").ToLowerInvariant());
        if (descriptor == null)
        {
            throw new ScenarioException(
                $"Unknown scenario '{name}'; available scenarios: {string.Join(", ", AllScenarios.Select(s => s.Name))}.");
        }

        return descriptor;
    }

    private static ScenarioInput IntInput(string name, string label, int min, int max, int def, string help)
    {
        return new ScenarioInput
        {
            Name = name,
            Label = label,
            Kind = "int",
            MinInt = min,
            MaxInt = max,
            Default = def.ToString(),
            Help = help
        };
    }

    private static ScenarioInput SeriesRateInput(string name, string label, double def, string help)
    {
        return new ScenarioInput
        {
            Name = name,
            Label = label,
            Kind = "rate",
            SeriesGrid = true,
            MinRate = 0.2,
            MaxRate = 22.0,
            Default = def.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Help = help
        };
    }

    private static ScenarioInput RootRateInput(string name, string label, double def, string help)
    {
        return new ScenarioInput
        {
            Name = name,
            Label = label,
            Kind = "rate",
            SeriesGrid = false,
            MinRate = 0.2,
            MaxRate = 20.0,
            Default = def.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Help = help
        };
    }

    private static ScenarioInput SeriesPayoutInput(string name, string label, double def, string help)
    {
        return new ScenarioInput
        {
            Name = name,
            Label = label,
            Kind = "rate",
            SeriesGrid = true,
            MinRate = 0.2,
            MaxRate = 6.0,
            Default = def.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Help = help
        };
    }

    private static ScenarioInput ChoiceInput(string name, string label, string[] choices, string def, string help)
    {
        return new ScenarioInput
        {
            Name = name,
            Label = label,
            Kind = "choice",
            Choices = choices,
            Default = def,
            Help = help
        };
    }
}