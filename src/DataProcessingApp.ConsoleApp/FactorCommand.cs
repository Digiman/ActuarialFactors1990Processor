using DataProcessingApp.Calculator;
using DataProcessingApp.Core.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DataProcessingApp.ConsoleApp;

/// <summary>
/// Options of the factor command: one scenario plus its inputs (age, rate,
/// payout, term, frequency...). Unset inputs fall back to the scenario
/// defaults from ScenarioCatalog.
/// </summary>
public class FactorSettings : CommandSettings
{
    [CommandOption("--scenario")]
    [Description("Scenario to compute: life-estate, annuity, unitrust, unitrust-two-life, annuity-trust-two-life, term-certain, term-unitrust, mortality.")]
    public string Scenario { get; set; }

    [CommandOption("--series")]
    [Description("Series for the scenario: 90CM, 2000CM or 2010CM (default 2010CM).")]
    public string Series { get; set; }

    [CommandOption("--age")]
    [Description("Age of the measuring life (0-109); also --age1/--age2 for two-life scenarios.")]
    public string Age { get; set; }

    [CommandOption("--age1")]
    [Description("Age of the older life for two-life scenarios (0-109).")]
    public string Age1 { get; set; }

    [CommandOption("--age2")]
    [Description("Age of the younger life for two-life scenarios (0-109).")]
    public string Age2 { get; set; }

    [CommandOption("--rate")]
    [Description("Section 7520 interest rate (%).")]
    public string Rate { get; set; }

    [CommandOption("--payout")]
    [Description("Unitrust payout rate (%).")]
    public string Payout { get; set; }

    [CommandOption("--years")]
    [Description("Term of years (term scenarios).")]
    public string Years { get; set; }

    [CommandOption("--frequency")]
    [Description("Payment frequency: Annual, Semiannual, Quarterly, Monthly (J/K annuities also Weekly).")]
    public string Frequency { get; set; }

    [CommandOption("--timing")]
    [Description("Annuity payment timing: Beginning (Table J) or End (Table K).")]
    public string Timing { get; set; }

    [CommandOption("--months")]
    [Description("Months from the valuation date to the first unitrust payout (Table F).")]
    public string Months { get; set; }
}

/// <summary>
/// Computes one actuarial scenario from the committed JSON data and prints
/// the result as a table: the exact published lookup(s) it is based on.
/// </summary>
public sealed class FactorCommand : Command<FactorSettings>
{
    protected override int Execute(CommandContext context, FactorSettings settings, System.Threading.CancellationToken cancellationToken)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

        if (string.IsNullOrEmpty(settings.Scenario))
        {
            PrintScenarios();
            return 1;
        }

        var calculator = new ActuarialCalculator(new FactorData(AppHelper.BaseDataDir));

        ScenarioResult result;
        try
        {
            result = calculator.Run(settings.Scenario, settings.Series, SettingsToArguments(settings));
        }
        catch (ScenarioException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Escape(ex.Message)}");
            return 1;
        }

        PrintResult(result);
        return 0;
    }

    private static ScenarioArguments SettingsToArguments(FactorSettings settings)
    {
        var values = new List<(string name, string value)>
        {
            ("age", settings.Age),
            ("age1", settings.Age1),
            ("age2", settings.Age2),
            ("rate", settings.Rate),
            ("payout", settings.Payout),
            ("years", settings.Years),
            ("frequency", settings.Frequency),
            ("timing", settings.Timing),
            ("months", settings.Months)
        };

        var provided = values.Where(v => !string.IsNullOrEmpty(v.value))
            .ToDictionary(v => v.name, v => v.value, StringComparer.OrdinalIgnoreCase);
        return new ScenarioArguments(provided);
    }

    private static void PrintResult(ScenarioResult result)
    {
        var header = result.Series == null ? result.Title : $"{result.Title} - {result.Series}";
        AnsiConsole.MarkupLine($"[bold green]{Escape(header)}[/]");
        AnsiConsole.MarkupLine($"[dim]{Escape(result.Purpose)}[/]");
        AnsiConsole.MarkupLine($"[dim]Source: {Escape(result.Citation)}[/]");

        var table = new Table().RoundedBorder().Expand();
        table.AddColumn("Value");
        table.AddColumn(new TableColumn("Factor").RightAligned());
        table.AddColumn("Based on");

        foreach (var value in result.Values)
        {
            table.AddRow(
                Escape(value.Label),
                Escape(value.Value.ToString("0.#####", CultureInfo.InvariantCulture)),
                Escape(value.Note ?? String.Empty));
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]IRS-published factors only: exact lookups, no interpolation.[/]");
    }

    private static void PrintScenarios()
    {
        AnsiConsole.MarkupLine("[yellow]Error: no scenario given.[/] Pass --scenario <name>; available scenarios:");
        var table = new Table().RoundedBorder();
        table.AddColumn("Scenario");
        table.AddColumn("Inputs");
        table.AddColumn("Purpose");

        foreach (var scenario in ScenarioCatalog.All)
        {
            var inputs = string.Join(" ", scenario.Inputs.Select(i => $"--{i.Name}"));
            table.AddRow(Escape(scenario.Name), Escape(inputs), Escape(scenario.Purpose));
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Example: factor --scenario life-estate --age 65 --rate 5.2 --series 2010CM[/]");
    }

    private static string Escape(string text)
    {
        return Markup.Escape(text ?? String.Empty);
    }
}