using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Logic.Loaders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataProcessingApp.Calculator;

/// <summary>
/// Cached read-only access to the actuarial tables used by the scenarios.
/// All tables are loaded from the committed JSON files; the two-life tables
/// are assembled from their five part files. Thread-safe (the web app shares
/// one instance).
/// </summary>
public sealed class FactorData
{
    private const string DefaultSeries = FilesHelper.Series2010CM;

    public static readonly string[] Series =
    {
        FilesHelper.Series90CM,
        FilesHelper.Series2000CM,
        FilesHelper.Series2010CM
    };

    // census year behind each series (Table S/H/C/U columns and MortalityTable)
    private static readonly Dictionary<string, int> CensusYears = new()
    {
        { FilesHelper.Series90CM, 1990 },
        { FilesHelper.Series2000CM, 2000 },
        { FilesHelper.Series2010CM, 2010 }
    };

    private readonly string _baseDir;
    private readonly ConcurrentDictionary<string, Lazy<object>> _cache = new();

    public FactorData() : this(null)
    {
    }

    /// <summary>baseDir defaults to AppHelper.BaseDataDir when null.</summary>
    public FactorData(string baseDir)
    {
        _baseDir = baseDir;
    }

    public string BaseDataDir => _baseDir ?? AppHelper.BaseDataDir;

    public List<TableSRow> TableS(string series) => Get<TableSRow>(TableType.TableS, series);

    public List<TableU1Row> TableU1(string series) => Get<TableU1Row>(TableType.TableU1, series);

    public List<TableBRow> TableB() => Get<TableBRow>(TableType.TableB, "");

    public List<TableDRow> TableD() => Get<TableDRow>(TableType.TableD, "");

    public List<TableFRow> TableF() => Get<TableFRow>(TableType.TableF, "");

    public List<TableJRow> TableJ() => Get<TableJRow>(TableType.TableJ, "");

    public List<TableKRow> TableK() => Get<TableKRow>(TableType.TableK, "");

    public List<TableU2Row> TableU2(string series) => GetParts<TableU2Row>(TableType.TableU2, series);

    public List<TableR2Row> TableR2(string series) => GetParts<TableR2Row>(TableType.TableR2, series);

    public List<MortalityTableRow> Mortality() => Get<MortalityTableRow>(TableType.MortalityTable, "");

    /// <summary>lx values for one census year (1980 / 1990 / 2000 / 2010).</summary>
    public List<MortalityTableRow> Mortality(int year)
    {
        var rows = Mortality().Where(r => r.Year == year).ToList();
        if (rows.Count == 0)
        {
            throw new ScenarioException($"MortalityTable has no rows for year {year}; published years: 1980, 1990, 2000, 2010.");
        }

        return rows;
    }

    public int CensusYear(string series)
    {
        ValidateSeries(series);
        return CensusYears[series];
    }

    public static void ValidateSeries(string series)
    {
        if (!CensusYears.ContainsKey(series ?? ""))
        {
            throw new ScenarioException($"Unknown series '{series}'; published series: {string.Join(", ", Series)}.");
        }
    }

    private List<TRow> Get<TRow>(TableType tableType, string series) where TRow : new()
    {
        var key = $"{tableType}|{series ?? ""}";
        var lazy = _cache.GetOrAdd(key, _ => new Lazy<object>(() => Load<TRow>(tableType, series)));
        return (List<TRow>)lazy.Value;
    }

    private List<TRow> Load<TRow>(TableType tableType, string series) where TRow : new()
    {
        if (!string.IsNullOrEmpty(series))
        {
            ValidateSeries(series);
        }

        var filename = FilesHelper.GenerateFilename(tableType, DocumentType.JSON, series, BaseDataDir);
        return new TableLoader<TRow>().LoadFromJson(filename);
    }

    private List<TRow> GetParts<TRow>(TableType tableType, string series) where TRow : new()
    {
        var key = $"{tableType}|parts|{series ?? ""}";
        var lazy = _cache.GetOrAdd(key, _ => new Lazy<object>(() => LoadParts<TRow>(tableType, series)));
        return (List<TRow>)lazy.Value;
    }

    private List<TRow> LoadParts<TRow>(TableType tableType, string series) where TRow : new()
    {
        ValidateSeries(series);

        var loader = new TableLoader<TRow>();
        var rows = new List<TRow>();
        foreach (var part in FilesHelper.PartFiles(tableType))
        {
            var filename = FilesHelper.GeneratePartFilename(part, series, BaseDataDir);
            if (!File.Exists(filename))
            {
                throw new ScenarioException($"Part file missing: {filename}. Two-life tables ship as five part files (recreate them with 'make data-json' or the C# 'json' workflow).");
            }

            rows.AddRange(loader.LoadFromJson(filename));
        }

        return rows;
    }
}