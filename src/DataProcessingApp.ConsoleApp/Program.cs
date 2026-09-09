using DataProcessingApp.ConsoleApp.Commands;
using Spectre.Console.Cli;

namespace DataProcessingApp.ConsoleApp;

/// <summary>
/// Main class for the console application. Workflow is selected by the first
/// argument (load | json | text | excel | database | all); run with --help
/// for the full option reference.
/// </summary>
static class Program
{
    static int Main(string[] args)
    {
        var app = new CommandApp<LoadCommand>();
        app.Configure(config =>
        {
            config.SetApplicationName("DataProcessingApp");
            config.SetApplicationVersion(typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "1.0");

            config.AddCommand<LoadCommand>("load").WithDescription("Load every table from its source files, all series (smoke check).");
            config.AddCommand<JsonCommand>("json").WithDescription("Load every root table and save it as JSON.");
            config.AddCommand<TextCommand>("text").WithDescription("Load every table, all series, and save it as a plain text file.");
            config.AddCommand<ExcelCommand>("excel").WithDescription("Load every table, all series, and save it as an Excel document.");
            config.AddCommand<DatabaseCommand>("database").WithDescription("Load every table, all series, and reload it into SQL Server.");
            config.AddCommand<AllCommand>("all").WithDescription("Run load, json, text and excel in sequence.");
        });

        return app.Run(args);
    }
}