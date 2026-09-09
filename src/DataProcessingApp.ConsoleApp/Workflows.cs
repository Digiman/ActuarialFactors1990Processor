using DataProcessingApp.ConsoleApp.Workers;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataProcessingApp.ConsoleApp;

public static class Workflows
{
    /// <summary>
    /// Series with committed data files (JSONFiles/&lt;series&gt;/ subfolders).
    /// Table Z exists for the 2000CM and 2010CM series (not 90CM).
    /// </summary>
    private static readonly string[] Series =
    {
        FilesHelper.Series90CM,
        FilesHelper.Series2000CM,
        FilesHelper.Series2010CM
    };

    /// <summary>Series that publish Table Z (unitrust commutation factors).</summary>
    private static readonly string[] SeriesWithTableZ =
    {
        FilesHelper.Series2000CM,
        FilesHelper.Series2010CM
    };

    /// <summary>
    /// Root tables loaded from XML (SQL Server export format), no series.
    /// </summary>
    private static readonly TableType[] RootTables =
    {
        TableType.TableK,
        TableType.TableJ,
        TableType.TableF,
        TableType.TableD,
        TableType.TableB,
        TableType.MortalityTable
    };

    /// <summary>
    /// One message per failed per-table step; empty when the workflow fully succeeded.
    /// </summary>
    public static readonly List<string> Failures = new();

    /// <summary>
    /// Loads every table from its source files (smoke check, writes nothing).
    /// Also the dry-run path of the saving workflows.
    /// </summary>
    public static void LoadData(RunOptions options)
    {
        if (options.DryRun)
        {
            Console.WriteLine("Dry run: validating source files, nothing will be written...");
        }
        else
        {
            Console.WriteLine("Load data from JSON files...");
        }

        ForEachSeries(options, series =>
        {
            Run(options, TableType.TableH, series, () => Worker<TableHRow>(TableType.TableH, series).LoadTableData());
            Run(options, TableType.TableS, series, () => Worker<TableSRow>(TableType.TableS, series).LoadTableData());
            Run(options, TableType.TableC, series, () => Worker<TableCRow>(TableType.TableC, series).LoadTableData());
            Run(options, TableType.TableU1, series, () => Worker<TableU1Row>(TableType.TableU1, series).LoadTableData());
            Run(options, TableType.TableU2, series, () => Worker<TableU2Row>(TableType.TableU2, series).LoadTableData());
            Run(options, TableType.TableR2, series, () => Worker<TableR2Row>(TableType.TableR2, series).LoadTableData());

            if (SeriesWithTableZ.Contains(series))
            {
                Run(options, TableType.TableZ, series, () => Worker<TableZRow>(TableType.TableZ, series).LoadTableData());
            }
        });

        if (RootTables.Any(options.Includes))
        {
            Console.WriteLine("Load data from XML files...");
        }

        Run(options, TableType.TableK, () => Worker<TableKRow>(TableType.TableK).LoadTableData());
        Run(options, TableType.TableJ, () => Worker<TableJRow>(TableType.TableJ).LoadTableData());
        Run(options, TableType.TableF, () => Worker<TableFRow>(TableType.TableF).LoadTableData());
        Run(options, TableType.TableD, () => Worker<TableDRow>(TableType.TableD).LoadTableData());
        Run(options, TableType.TableB, () => Worker<TableBRow>(TableType.TableB).LoadTableData());
        Run(options, TableType.MortalityTable, () => Worker<MortalityTableRow>(TableType.MortalityTable).LoadTableData());
    }

    /// <summary>
    /// Loads every root table (XML-based) and saves it as a JSON file.
    /// </summary>
    public static void SaveToJsonFiles(RunOptions options)
    {
        if (WriteSkippedInDryRun(options, "JSON"))
        {
            LoadData(options);
            return;
        }

        Console.WriteLine("Save to JSON files...");

        Run(options, TableType.TableK, () => Worker<TableKRow>(TableType.TableK).SaveToJsonFile());
        Run(options, TableType.TableJ, () => Worker<TableJRow>(TableType.TableJ).SaveToJsonFile());
        Run(options, TableType.TableF, () => Worker<TableFRow>(TableType.TableF).SaveToJsonFile());
        Run(options, TableType.TableD, () => Worker<TableDRow>(TableType.TableD).SaveToJsonFile());
        Run(options, TableType.TableB, () => Worker<TableBRow>(TableType.TableB).SaveToJsonFile());
        Run(options, TableType.MortalityTable, () => Worker<MortalityTableRow>(TableType.MortalityTable).SaveToJsonFile());
    }

    /// <summary>
    /// Loads every table and saves it as a plain text file.
    /// </summary>
    public static void SaveToTextFiles(RunOptions options)
    {
        if (WriteSkippedInDryRun(options, "text"))
        {
            LoadData(options);
            return;
        }

        Console.WriteLine("Save to text files...");

        ForEachSeries(options, series =>
        {
            Run(options, TableType.TableH, series, () => Worker<TableHRow>(TableType.TableH, series).SaveToTextFile());
            Run(options, TableType.TableS, series, () => Worker<TableSRow>(TableType.TableS, series).SaveToTextFile());
            Run(options, TableType.TableC, series, () => Worker<TableCRow>(TableType.TableC, series).SaveToTextFile());
            Run(options, TableType.TableU1, series, () => Worker<TableU1Row>(TableType.TableU1, series).SaveToTextFile());
            Run(options, TableType.TableU2, series, () => Worker<TableU2Row>(TableType.TableU2, series).CombineTableParts());
            Run(options, TableType.TableU2, series, () => Worker<TableU2Row>(TableType.TableU2, series).SaveToTextFile());
            Run(options, TableType.TableR2, series, () => Worker<TableR2Row>(TableType.TableR2, series).CombineTableParts());
            Run(options, TableType.TableR2, series, () => Worker<TableR2Row>(TableType.TableR2, series).SaveToTextFile());

            if (SeriesWithTableZ.Contains(series))
            {
                Run(options, TableType.TableZ, series, () => Worker<TableZRow>(TableType.TableZ, series).SaveToTextFile());
            }
        });
    }

    /// <summary>
    /// Loads every table and saves it as an Excel document.
    /// </summary>
    public static void ExportToExcel(RunOptions options)
    {
        if (WriteSkippedInDryRun(options, "Excel"))
        {
            LoadData(options);
            return;
        }

        Console.WriteLine("Save to Excel files...");

        ForEachSeries(options, series =>
        {
            Run(options, TableType.TableH, series, () => Worker<TableHRow>(TableType.TableH, series).ExportToExcel());
            Run(options, TableType.TableS, series, () => Worker<TableSRow>(TableType.TableS, series).ExportToExcel());
            Run(options, TableType.TableC, series, () => Worker<TableCRow>(TableType.TableC, series).ExportToExcel());
            Run(options, TableType.TableU1, series, () => Worker<TableU1Row>(TableType.TableU1, series).ExportToExcel());
            Run(options, TableType.TableU2, series, () => Worker<TableU2Row>(TableType.TableU2, series).CombineTableParts());
            Run(options, TableType.TableU2, series, () => Worker<TableU2Row>(TableType.TableU2, series).ExportToExcel());
            Run(options, TableType.TableR2, series, () => Worker<TableR2Row>(TableType.TableR2, series).CombineTableParts());
            Run(options, TableType.TableR2, series, () => Worker<TableR2Row>(TableType.TableR2, series).ExportToExcel());

            if (SeriesWithTableZ.Contains(series))
            {
                Run(options, TableType.TableZ, series, () => Worker<TableZRow>(TableType.TableZ, series).ExportToExcel());
            }
        });

        //---------------------------------------

        Run(options, TableType.TableK, () => Worker<TableKRow>(TableType.TableK).ExportToExcel());
        Run(options, TableType.TableJ, () => Worker<TableJRow>(TableType.TableJ).ExportToExcel());
        Run(options, TableType.TableF, () => Worker<TableFRow>(TableType.TableF).ExportToExcel());
        Run(options, TableType.TableD, () => Worker<TableDRow>(TableType.TableD).ExportToExcel());
        Run(options, TableType.TableB, () => Worker<TableBRow>(TableType.TableB).ExportToExcel());
        Run(options, TableType.MortalityTable, () => Worker<MortalityTableRow>(TableType.MortalityTable).ExportToExcel());
    }

    /// <summary>
    /// Loads every table and reloads it into the SQL Server destination tables
    /// (existing rows are cleared first, so reruns are idempotent).
    /// </summary>
    public static void SaveToDatabase(RunOptions options)
    {
        if (WriteSkippedInDryRun(options, "database"))
        {
            LoadData(options);
            return;
        }

        Console.WriteLine("Save data to database...");

        ForEachSeries(options, series =>
        {
            Run(options, TableType.TableH, series, () => Worker<TableHRow>(TableType.TableH, series).SaveToDatabase());
            Run(options, TableType.TableS, series, () => Worker<TableSRow>(TableType.TableS, series).SaveToDatabase());
            Run(options, TableType.TableC, series, () => Worker<TableCRow>(TableType.TableC, series).SaveToDatabase());
            Run(options, TableType.TableU1, series, () => Worker<TableU1Row>(TableType.TableU1, series).SaveToDatabase());
            Run(options, TableType.TableU2, series, () => Worker<TableU2Row>(TableType.TableU2, series).CombineTableParts());
            Run(options, TableType.TableU2, series, () => Worker<TableU2Row>(TableType.TableU2, series).SaveToDatabase());
            Run(options, TableType.TableR2, series, () => Worker<TableR2Row>(TableType.TableR2, series).CombineTableParts());
            Run(options, TableType.TableR2, series, () => Worker<TableR2Row>(TableType.TableR2, series).SaveToDatabase());

            if (SeriesWithTableZ.Contains(series))
            {
                Run(options, TableType.TableZ, series, () => Worker<TableZRow>(TableType.TableZ, series).SaveToDatabase());
            }
        });

        //---------------------------------------

        Run(options, TableType.TableK, () => Worker<TableKRow>(TableType.TableK).SaveToDatabase());
        Run(options, TableType.TableJ, () => Worker<TableJRow>(TableType.TableJ).SaveToDatabase());
        Run(options, TableType.TableF, () => Worker<TableFRow>(TableType.TableF).SaveToDatabase());
        Run(options, TableType.TableD, () => Worker<TableDRow>(TableType.TableD).SaveToDatabase());
        Run(options, TableType.TableB, () => Worker<TableBRow>(TableType.TableB).SaveToDatabase());
        Run(options, TableType.MortalityTable, () => Worker<MortalityTableRow>(TableType.MortalityTable).SaveToDatabase());
    }

    /// <summary>
    /// Runs load, json, text and excel in sequence. In dry-run mode the source
    /// files are validated once and the saving workflows are skipped.
    /// </summary>
    public static void RunAll(RunOptions options)
    {
        if (options.DryRun)
        {
            LoadData(options);
            Console.WriteLine("Dry run: json, text and excel workflows would save the validated tables.");
            return;
        }

        LoadData(options);
        SaveToJsonFiles(options);
        SaveToTextFiles(options);
        ExportToExcel(options);
    }

    private static bool WriteSkippedInDryRun(RunOptions options, string target)
    {
        if (!options.DryRun)
        {
            return false;
        }

        Console.WriteLine("Dry run: {0} workflow would write here, validating sources instead...", target);
        return true;
    }

    /// <summary>
    /// Runs one per-table step with error isolation: a filtered-out table is
    /// skipped, a failed table is reported and the workflow continues.
    /// </summary>
    private static void Run(RunOptions options, TableType tableType, string series, Action action)
    {
        if (!options.Includes(tableType))
        {
            return;
        }

        var label = Label(tableType, series);
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Failures.Add($"{label}: {ex.Message}");
            Console.WriteLine("Processing {0} - FAILED: {1}", label, ex.Message);
        }
    }

    private static void Run(RunOptions options, TableType tableType, Action action)
    {
        Run(options, tableType, String.Empty, action);
    }

    /// <summary>
    /// Resolves the series to iterate: all series when no filter is given;
    /// throws for unknown filter values.
    /// </summary>
    private static IEnumerable<string> SeriesToRun(RunOptions options)
    {
        if (options.SeriesFilter.Length == 0)
        {
            return Series;
        }

        foreach (var requested in options.SeriesFilter)
        {
            if (!Series.Contains(requested, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Unknown series '{requested}'. Supported: {String.Join(", ", Series)} (2000CM has no data yet).");
            }
        }

        return Series.Where(s => options.SeriesFilter.Contains(s, StringComparer.OrdinalIgnoreCase));
    }

    private static void ForEachSeries(RunOptions options, Action<string> action)
    {
        foreach (var series in SeriesToRun(options))
        {
            action(series);
        }
    }

    private static string Label(TableType tableType, string series = "")
    {
        var seriesSuffix = String.IsNullOrEmpty(series) ? "" : $" ({series})";
        return $"{FilesHelper.TableDisplayName(tableType)}{seriesSuffix}";
    }

    private static TableWorker<TRow> Worker<TRow>(TableType tableType, string series = "") where TRow : new()
    {
        return new TableWorker<TRow>(tableType, series);
    }
}