using DataProcessingApp.Core.Helpers;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;

namespace DataProcessingApp.ConsoleApp;

/// <summary>
/// Options shared by every workflow command: optional series/table filters
/// and dry-run mode.
/// </summary>
public class WorkflowSettings : CommandSettings
{
    [CommandOption("--series")]
    [Description("Restrict series tables to these series (comma-separated): 90CM, 2010CM.")]
    public string[] Series { get; set; } = Array.Empty<string>();

    [CommandOption("--table")]
    [Description("Restrict processing to these tables (comma-separated), e.g. --table S,K or --table MortalityTable.")]
    public string[] Tables { get; set; } = Array.Empty<string>();

    [CommandOption("--dry-run")]
    [Description("Load and validate the source files but write nothing.")]
    public bool DryRun { get; set; }

    public RunOptions ToRunOptions()
    {
        return new RunOptions(Series, Tables, DryRun);
    }
}