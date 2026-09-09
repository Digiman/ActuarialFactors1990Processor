using DataProcessingApp.Core.Helpers;
using Spectre.Console.Cli;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace DataProcessingApp.ConsoleApp.Commands;

/// <summary>
/// Base class for the workflow commands: runs one phase with per-table error
/// isolation and phase timing, and translates failures into the exit code.
/// </summary>
public abstract class WorkflowCommand<TSettings> : Command<TSettings> where TSettings : WorkflowSettings
{
    protected override int Execute(CommandContext context, TSettings settings, CancellationToken cancellationToken)
    {
        CultureFix();

        RunOptions options;
        try
        {
            options = settings.ToRunOptions();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            return 1;
        }

        var timer = Stopwatch.StartNew();
        try
        {
            ExecuteWorkflow(options);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            return 1;
        }
        timer.Stop();

        Console.WriteLine(PhaseName + " time: {0} ms", timer.ElapsedMilliseconds);

        if (Workflows.Failures.Count > 0)
        {
            Console.WriteLine("{0} step(s) failed:", Workflows.Failures.Count);
            foreach (var failure in Workflows.Failures)
            {
                Console.WriteLine("  - " + failure);
            }
            return 1;
        }

        return 0;
    }

    /// <summary>Human-readable phase name for the timing line.</summary>
    protected abstract string PhaseName { get; }

    /// <summary>Runs the actual workflow for the parsed options.</summary>
    protected abstract void ExecuteWorkflow(RunOptions options);

    private static void CultureFix()
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
    }
}

/// <summary>Loads every table from its source files (smoke check, writes nothing).</summary>
public sealed class LoadCommand : WorkflowCommand<WorkflowSettings>
{
    protected override string PhaseName => "Loading tables";

    protected override void ExecuteWorkflow(RunOptions options)
    {
        Workflows.LoadData(options);
    }
}

/// <summary>Loads every root table and saves it as JSON.</summary>
public sealed class JsonCommand : WorkflowCommand<WorkflowSettings>
{
    protected override string PhaseName => "Saving to JSON";

    protected override void ExecuteWorkflow(RunOptions options)
    {
        Workflows.SaveToJsonFiles(options);
    }
}

/// <summary>Loads every table and saves it as a plain text file.</summary>
public sealed class TextCommand : WorkflowCommand<WorkflowSettings>
{
    protected override string PhaseName => "Saving to text";

    protected override void ExecuteWorkflow(RunOptions options)
    {
        Workflows.SaveToTextFiles(options);
    }
}

/// <summary>Loads every table and saves it as an Excel document.</summary>
public sealed class ExcelCommand : WorkflowCommand<WorkflowSettings>
{
    protected override string PhaseName => "Saving to Excel";

    protected override void ExecuteWorkflow(RunOptions options)
    {
        Workflows.ExportToExcel(options);
    }
}

/// <summary>Loads every table and reloads it into SQL Server.</summary>
public sealed class DatabaseCommand : WorkflowCommand<WorkflowSettings>
{
    protected override string PhaseName => "Database copy";

    protected override void ExecuteWorkflow(RunOptions options)
    {
        Workflows.SaveToDatabase(options);
    }
}

/// <summary>Runs load, json, text and excel in sequence.</summary>
public sealed class AllCommand : WorkflowCommand<WorkflowSettings>
{
    protected override string PhaseName => "All workflows";

    protected override void ExecuteWorkflow(RunOptions options)
    {
        Workflows.RunAll(options);
    }
}