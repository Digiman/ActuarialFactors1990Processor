using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Data.Repositories;
using DataProcessingApp.Logic.Loaders;
using DataProcessingApp.Logic.Savers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DataProcessingApp.ConsoleApp.Workers;

/// <summary>
/// Generic worker that orchestrates load/save operations for a single actuarial table.
/// </summary>
public class TableWorker<TRow> where TRow : new()
{
    private readonly TableType _tableType;
    private readonly string _series;

    public TableWorker(TableType tableType, string series = "")
    {
        _tableType = tableType;
        _series = series;
    }

    /// <summary>
    /// Loads table data without saving it anywhere (smoke check of the source files).
    /// </summary>
    public void LoadTableData()
    {
        LogStart();

        var timer = StartTiming();

        var loader = new TableLoader<TRow>();
        var parts = FilesHelper.PartFiles(_tableType);
        if (parts != null)
        {
            var total = 0;
            foreach (var part in parts)
            {
                total += loader.LoadFromJson(FilesHelper.GeneratePartFilename(part, _series)).Count;
            }
            LogEnd(total, timer);
        }
        else
        {
            var rows = LoadTable(loader);
            LogEnd(rows.Count, timer);
        }
    }

    /// <summary>
    /// Loads table data from its native format and saves it as a JSON file.
    /// </summary>
    public void SaveToJsonFile()
    {
        LogStart();

        var timer = StartTiming();
        var rows = LoadTable(new TableLoader<TRow>());

        var resultFilename = FilesHelper.GenerateFilename(_tableType, DocumentType.JSON, _series);
        new TableSaver<TRow>().SaveToJsonFile(resultFilename, rows);

        LogEnd(rows.Count, timer);
    }

    /// <summary>
    /// Combines per-part JSON files into one combined JSON file
    /// (only for tables that are split into parts; no-op otherwise).
    /// </summary>
    public void CombineTableParts()
    {
        var parts = FilesHelper.PartFiles(_tableType);
        if (parts == null)
        {
            return;
        }

        var jsonFilename = FilesHelper.GenerateFilename(_tableType, DocumentType.JSON, _series);
        if (File.Exists(jsonFilename))
        {
            return;
        }

        LogStart();

        var timer = StartTiming();

        var loader = new TableLoader<TRow>();
        var combined = new List<TRow>();
        foreach (var part in parts)
        {
            combined.AddRange(loader.LoadFromJson(FilesHelper.GeneratePartFilename(part, _series)));
        }

        new TableSaver<TRow>().SaveToJsonFile(jsonFilename, combined);

        LogEnd(combined.Count, timer);
    }

    /// <summary>
    /// Loads table data and saves it as an Excel document.
    /// </summary>
    public void ExportToExcel()
    {
        LogStart();

        var timer = StartTiming();
        var rows = LoadTable(new TableLoader<TRow>());

        var excelFilename = FilesHelper.GenerateFilename(_tableType, DocumentType.Excel, _series);
        new TableSaver<TRow>().SaveToExcel(FilesHelper.TableDisplayName(_tableType), excelFilename, rows);

        LogEnd(rows.Count, timer);
    }

    /// <summary>
    /// Loads table data and saves it as a plain text file.
    /// </summary>
    public void SaveToTextFile()
    {
        LogStart();

        var timer = StartTiming();
        var rows = LoadTable(new TableLoader<TRow>());

        var textFilename = FilesHelper.GenerateFilename(_tableType, DocumentType.Text, _series);
        new TableSaver<TRow>().SaveToTextFile(textFilename, rows);

        LogEnd(rows.Count, timer);
    }

    /// <summary>
    /// Loads table data and reloads it into the SQL Server destination table
    /// (existing rows are cleared first, so reruns are idempotent).
    /// </summary>
    public void SaveToDatabase()
    {
        LogStart();

        var timer = StartTiming();
        var rows = LoadTable(new TableLoader<TRow>());

        var repository = new TableRepository<TRow>(AppHelper.DatabaseConnectionString, _tableType);
        var inserted = repository.InsertTableData(rows);

        LogEnd(rows.Count, timer, inserted);
    }

    private List<TRow> LoadTable(TableLoader<TRow> loader)
    {
        return FilesHelper.IsXmlBased(_tableType)
            ? loader.LoadFromXml(FilesHelper.GenerateFilename(_tableType, DocumentType.XML, _series))
            : loader.LoadFromJson(FilesHelper.GenerateFilename(_tableType, DocumentType.JSON, _series));
    }

    private string TableLabel()
    {
        var seriesSuffix = String.IsNullOrEmpty(_series) ? "" : $" ({_series})";
        return $"{FilesHelper.TableDisplayName(_tableType)}{seriesSuffix}";
    }

    private void LogStart()
    {
        Console.WriteLine("Processing {0}...", TableLabel());
    }

    private static Stopwatch StartTiming()
    {
        var timer = new Stopwatch();
        timer.Start();
        return timer;
    }

    private static string FormatCount(int records)
    {
        return records.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
    }

    private void LogEnd(int records, Stopwatch timer)
    {
        Console.WriteLine("Processing {0} - done, {1} records in {2} ms.", TableLabel(), FormatCount(records), timer.ElapsedMilliseconds);
    }

    private void LogEnd(int loaded, Stopwatch timer, int inserted)
    {
        Console.WriteLine("Processing {0} - done, {1} records loaded, {2} inserted in {3} ms.", TableLabel(), FormatCount(loaded), FormatCount(inserted), timer.ElapsedMilliseconds);
    }
}
