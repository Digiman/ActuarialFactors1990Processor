using DataProcessingApp.ConsoleApp.Workers;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using System;
using System.Collections.Generic;

namespace DataProcessingApp.ConsoleApp;

public static class Workflows
{
    /// <summary>
    /// Series with committed data files (JSONFiles/&lt;series&gt;/ subfolders).
    /// Table Z exists only for the 2010CM series.
    /// </summary>
    private static readonly string[] Series =
    {
        FilesHelper.Series90CM,
        FilesHelper.Series2010CM
    };

    /// <summary>
    /// One message per failed per-table step; empty when the workflow fully succeeded.
    /// </summary>
    public static readonly List<string> Failures = new();

    /// <summary>
    /// Runs one per-table step with error isolation: a failed table is reported
    /// and the workflow continues with the next table.
    /// </summary>
    private static void Run(string label, Action action)
    {
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

    private static string Label(TableType tableType, string series = "")
    {
        var seriesSuffix = String.IsNullOrEmpty(series) ? "" : $" ({series})";
        return $"{FilesHelper.TableDisplayName(tableType)}{seriesSuffix}";
    }

    public static void LoaderTestsReadFromJson()
    {
        Console.WriteLine("Load data from JSON files...");

        ForEachSeries(series =>
        {
            Run(Label(TableType.TableH, series), () => Worker<TableHRow>(TableType.TableH, series).LoadTableData());
            Run(Label(TableType.TableS, series), () => Worker<TableSRow>(TableType.TableS, series).LoadTableData());
            Run(Label(TableType.TableC, series), () => Worker<TableCRow>(TableType.TableC, series).LoadTableData());
            Run(Label(TableType.TableU1, series), () => Worker<TableU1Row>(TableType.TableU1, series).LoadTableData());
            Run(Label(TableType.TableU2, series), () => Worker<TableU2Row>(TableType.TableU2, series).LoadTableData());
            Run(Label(TableType.TableR2, series), () => Worker<TableR2Row>(TableType.TableR2, series).LoadTableData());

            if (series == FilesHelper.Series2010CM)
            {
                Run(Label(TableType.TableZ, series), () => Worker<TableZRow>(TableType.TableZ, series).LoadTableData());
            }
        });
    }

    public static void LoaderTestsReadFromXml()
    {
        Console.WriteLine("Load data from XML files...");

        Run(Label(TableType.TableK), () => Worker<TableKRow>(TableType.TableK).LoadTableData());
        Run(Label(TableType.TableJ), () => Worker<TableJRow>(TableType.TableJ).LoadTableData());
        Run(Label(TableType.TableF), () => Worker<TableFRow>(TableType.TableF).LoadTableData());
        Run(Label(TableType.TableD), () => Worker<TableDRow>(TableType.TableD).LoadTableData());
        Run(Label(TableType.TableB), () => Worker<TableBRow>(TableType.TableB).LoadTableData());
        Run(Label(TableType.MortalityTable), () => Worker<MortalityTableRow>(TableType.MortalityTable).LoadTableData());
    }

    public static void TextFileSaverTests()
    {
        Console.WriteLine("Save to text files...");

        ForEachSeries(series =>
        {
            Run(Label(TableType.TableH, series), () => Worker<TableHRow>(TableType.TableH, series).SaveToTextFile());
            Run(Label(TableType.TableS, series), () => Worker<TableSRow>(TableType.TableS, series).SaveToTextFile());
            Run(Label(TableType.TableC, series), () => Worker<TableCRow>(TableType.TableC, series).SaveToTextFile());
            Run(Label(TableType.TableU1, series), () => Worker<TableU1Row>(TableType.TableU1, series).SaveToTextFile());
            Run(Label(TableType.TableU2, series), () => Worker<TableU2Row>(TableType.TableU2, series).CombineTableParts());
            Run(Label(TableType.TableU2, series), () => Worker<TableU2Row>(TableType.TableU2, series).SaveToTextFile());
            Run(Label(TableType.TableR2, series), () => Worker<TableR2Row>(TableType.TableR2, series).CombineTableParts());
            Run(Label(TableType.TableR2, series), () => Worker<TableR2Row>(TableType.TableR2, series).SaveToTextFile());

            if (series == FilesHelper.Series2010CM)
            {
                Run(Label(TableType.TableZ, series), () => Worker<TableZRow>(TableType.TableZ, series).SaveToTextFile());
            }
        });
    }

    /// <summary>
    /// Save all data table to Excel files.
    /// </summary>
    public static void ExcelSaverTests()
    {
        Console.WriteLine("Save to Excel files...");

        ForEachSeries(series =>
        {
            Run(Label(TableType.TableH, series), () => Worker<TableHRow>(TableType.TableH, series).ExportToExcel());
            Run(Label(TableType.TableS, series), () => Worker<TableSRow>(TableType.TableS, series).ExportToExcel());
            Run(Label(TableType.TableC, series), () => Worker<TableCRow>(TableType.TableC, series).ExportToExcel());
            Run(Label(TableType.TableU1, series), () => Worker<TableU1Row>(TableType.TableU1, series).ExportToExcel());
            Run(Label(TableType.TableU2, series), () => Worker<TableU2Row>(TableType.TableU2, series).CombineTableParts());
            Run(Label(TableType.TableU2, series), () => Worker<TableU2Row>(TableType.TableU2, series).ExportToExcel());
            Run(Label(TableType.TableR2, series), () => Worker<TableR2Row>(TableType.TableR2, series).CombineTableParts());
            Run(Label(TableType.TableR2, series), () => Worker<TableR2Row>(TableType.TableR2, series).ExportToExcel());

            if (series == FilesHelper.Series2010CM)
            {
                Run(Label(TableType.TableZ, series), () => Worker<TableZRow>(TableType.TableZ, series).ExportToExcel());
            }
        });

        //---------------------------------------

        Run(Label(TableType.TableK), () => Worker<TableKRow>(TableType.TableK).ExportToExcel());
        Run(Label(TableType.TableJ), () => Worker<TableJRow>(TableType.TableJ).ExportToExcel());
        Run(Label(TableType.TableF), () => Worker<TableFRow>(TableType.TableF).ExportToExcel());
        Run(Label(TableType.TableD), () => Worker<TableDRow>(TableType.TableD).ExportToExcel());
        Run(Label(TableType.TableB), () => Worker<TableBRow>(TableType.TableB).ExportToExcel());
        Run(Label(TableType.MortalityTable), () => Worker<MortalityTableRow>(TableType.MortalityTable).ExportToExcel());
    }

    public static void JsonFileSaverTests()
    {
        Console.WriteLine("Save to JSON files...");

        Run(Label(TableType.TableK), () => Worker<TableKRow>(TableType.TableK).SaveToJsonFile());
        Run(Label(TableType.TableJ), () => Worker<TableJRow>(TableType.TableJ).SaveToJsonFile());
        Run(Label(TableType.TableF), () => Worker<TableFRow>(TableType.TableF).SaveToJsonFile());
        Run(Label(TableType.TableD), () => Worker<TableDRow>(TableType.TableD).SaveToJsonFile());
        Run(Label(TableType.TableB), () => Worker<TableBRow>(TableType.TableB).SaveToJsonFile());
        Run(Label(TableType.MortalityTable), () => Worker<MortalityTableRow>(TableType.MortalityTable).SaveToJsonFile());
    }

    /// <summary>
    /// Full tests for all tables data to reload into database with bulk insert.
    /// </summary>
    public static void DatabaseTests()
    {
        Console.WriteLine("Save data to database...");

        ForEachSeries(series =>
        {
            Run(Label(TableType.TableH, series), () => Worker<TableHRow>(TableType.TableH, series).SaveToDatabase());
            Run(Label(TableType.TableS, series), () => Worker<TableSRow>(TableType.TableS, series).SaveToDatabase());
            Run(Label(TableType.TableC, series), () => Worker<TableCRow>(TableType.TableC, series).SaveToDatabase());
            Run(Label(TableType.TableU1, series), () => Worker<TableU1Row>(TableType.TableU1, series).SaveToDatabase());
            Run(Label(TableType.TableU2, series), () => Worker<TableU2Row>(TableType.TableU2, series).CombineTableParts());
            Run(Label(TableType.TableU2, series), () => Worker<TableU2Row>(TableType.TableU2, series).SaveToDatabase());
            Run(Label(TableType.TableR2, series), () => Worker<TableR2Row>(TableType.TableR2, series).CombineTableParts());
            Run(Label(TableType.TableR2, series), () => Worker<TableR2Row>(TableType.TableR2, series).SaveToDatabase());

            if (series == FilesHelper.Series2010CM)
            {
                Run(Label(TableType.TableZ, series), () => Worker<TableZRow>(TableType.TableZ, series).SaveToDatabase());
            }
        });

        //---------------------------------------

        Run(Label(TableType.TableK), () => Worker<TableKRow>(TableType.TableK).SaveToDatabase());
        Run(Label(TableType.TableJ), () => Worker<TableJRow>(TableType.TableJ).SaveToDatabase());
        Run(Label(TableType.TableF), () => Worker<TableFRow>(TableType.TableF).SaveToDatabase());
        Run(Label(TableType.TableD), () => Worker<TableDRow>(TableType.TableD).SaveToDatabase());
        Run(Label(TableType.TableB), () => Worker<TableBRow>(TableType.TableB).SaveToDatabase());
        Run(Label(TableType.MortalityTable), () => Worker<MortalityTableRow>(TableType.MortalityTable).SaveToDatabase());
    }

    private static void ForEachSeries(Action<string> action)
    {
        foreach (var series in Series)
        {
            action(series);
        }
    }

    private static TableWorker<TRow> Worker<TRow>(TableType tableType, string series = "") where TRow : new()
    {
        return new TableWorker<TRow>(tableType, series);
    }
}