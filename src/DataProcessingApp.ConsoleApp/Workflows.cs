using DataProcessingApp.ConsoleApp.Workers;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using System;

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

    public static void LoaderTestsReadFromJson()
    {
        Console.WriteLine("Load data from JSON files...");

        ForEachSeries(series =>
        {
            Worker<TableHRow>(TableType.TableH, series).LoadTableData();
            Worker<TableSRow>(TableType.TableS, series).LoadTableData();
            Worker<TableCRow>(TableType.TableC, series).LoadTableData();
            Worker<TableU1Row>(TableType.TableU1, series).LoadTableData();
            Worker<TableU2Row>(TableType.TableU2, series).LoadTableData();
            Worker<TableR2Row>(TableType.TableR2, series).LoadTableData();

            if (series == FilesHelper.Series2010CM)
            {
                Worker<TableZRow>(TableType.TableZ, series).LoadTableData();
            }
        });
    }

    public static void LoaderTestsReadFromXml()
    {
        Console.WriteLine("Load data from XML files...");

        Worker<TableKRow>(TableType.TableK).LoadTableData();
        Worker<TableJRow>(TableType.TableJ).LoadTableData();
        Worker<TableFRow>(TableType.TableF).LoadTableData();
        Worker<TableDRow>(TableType.TableD).LoadTableData();
        Worker<TableBRow>(TableType.TableB).LoadTableData();
        Worker<MortalityTableRow>(TableType.MortalityTable).LoadTableData();
    }

    public static void TextFileSaverTests()
    {
        Console.WriteLine("Save to text files...");

        ForEachSeries(series =>
        {
            Worker<TableHRow>(TableType.TableH, series).SaveToTextFile();
            Worker<TableSRow>(TableType.TableS, series).SaveToTextFile();
            Worker<TableCRow>(TableType.TableC, series).SaveToTextFile();
            Worker<TableU1Row>(TableType.TableU1, series).SaveToTextFile();
            Worker<TableU2Row>(TableType.TableU2, series).CombineTableParts();
            Worker<TableU2Row>(TableType.TableU2, series).SaveToTextFile();
            Worker<TableR2Row>(TableType.TableR2, series).CombineTableParts();
            Worker<TableR2Row>(TableType.TableR2, series).SaveToTextFile();

            if (series == FilesHelper.Series2010CM)
            {
                Worker<TableZRow>(TableType.TableZ, series).SaveToTextFile();
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
            Worker<TableHRow>(TableType.TableH, series).ExportToExcel();
            Worker<TableSRow>(TableType.TableS, series).ExportToExcel();
            Worker<TableCRow>(TableType.TableC, series).ExportToExcel();
            Worker<TableU1Row>(TableType.TableU1, series).ExportToExcel();
            Worker<TableU2Row>(TableType.TableU2, series).CombineTableParts();
            Worker<TableU2Row>(TableType.TableU2, series).ExportToExcel();
            Worker<TableR2Row>(TableType.TableR2, series).CombineTableParts();
            Worker<TableR2Row>(TableType.TableR2, series).ExportToExcel();

            if (series == FilesHelper.Series2010CM)
            {
                Worker<TableZRow>(TableType.TableZ, series).ExportToExcel();
            }
        });

        //---------------------------------------

        Worker<TableKRow>(TableType.TableK).ExportToExcel();
        Worker<TableJRow>(TableType.TableJ).ExportToExcel();
        Worker<TableFRow>(TableType.TableF).ExportToExcel();
        Worker<TableDRow>(TableType.TableD).ExportToExcel();
        Worker<TableBRow>(TableType.TableB).ExportToExcel();
        Worker<MortalityTableRow>(TableType.MortalityTable).ExportToExcel();
    }

    public static void JsonFileSaverTests()
    {
        Console.WriteLine("Save to JSON files...");

        Worker<TableKRow>(TableType.TableK).SaveToJsonFile();
        Worker<TableJRow>(TableType.TableJ).SaveToJsonFile();
        Worker<TableFRow>(TableType.TableF).SaveToJsonFile();
        Worker<TableDRow>(TableType.TableD).SaveToJsonFile();
        Worker<TableBRow>(TableType.TableB).SaveToJsonFile();
        Worker<MortalityTableRow>(TableType.MortalityTable).SaveToJsonFile();
    }

    /// <summary>
    /// Full tests for all tables data to save in database with bulk insert.
    /// </summary>
    public static void DatabaseTests()
    {
        Console.WriteLine("Save data to database...");

        ForEachSeries(series =>
        {
            Worker<TableHRow>(TableType.TableH, series).SaveToDatabase();
            Worker<TableSRow>(TableType.TableS, series).SaveToDatabase();
            Worker<TableCRow>(TableType.TableC, series).SaveToDatabase();
            Worker<TableU1Row>(TableType.TableU1, series).SaveToDatabase();
            Worker<TableU2Row>(TableType.TableU2, series).CombineTableParts();
            Worker<TableU2Row>(TableType.TableU2, series).SaveToDatabase();
            Worker<TableR2Row>(TableType.TableR2, series).CombineTableParts();
            Worker<TableR2Row>(TableType.TableR2, series).SaveToDatabase();

            if (series == FilesHelper.Series2010CM)
            {
                Worker<TableZRow>(TableType.TableZ, series).SaveToDatabase();
            }
        });

        //---------------------------------------

        Worker<TableKRow>(TableType.TableK).SaveToDatabase();
        Worker<TableJRow>(TableType.TableJ).SaveToDatabase();
        Worker<TableFRow>(TableType.TableF).SaveToDatabase();
        Worker<TableDRow>(TableType.TableD).SaveToDatabase();
        Worker<TableBRow>(TableType.TableB).SaveToDatabase();
        Worker<MortalityTableRow>(TableType.MortalityTable).SaveToDatabase();
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
