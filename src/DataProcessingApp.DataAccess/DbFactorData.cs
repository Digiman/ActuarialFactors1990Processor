using DataProcessingApp.Calculator;
using DataProcessingApp.Core.DataObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DataProcessingApp.DataAccess;

/// <summary>
/// Cached read-only access to the actuarial tables from the SQL Server
/// database via EF Core; the database-backed <see cref="FactorDataSource"/>
/// for the web runtime. Table rows are the shared Core types, loaded
/// no-tracking (the tables have no keys). Thread-safe, one instance is
/// shared by the whole app.
/// </summary>
public sealed class DbFactorData : FactorDataSource
{
    private readonly string _connectionString;
    private readonly ConcurrentDictionary<string, Lazy<object>> _cache = new();

    public DbFactorData(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<TableSRow> TableS(string series) => Get<TableSRow>(series, r => r.MortalityTable);

    public List<TableU1Row> TableU1(string series) => Get<TableU1Row>(series, r => r.MortalityTable);

    public List<TableU2Row> TableU2(string series) => Get<TableU2Row>(series, r => r.MortalityTable);

    public List<TableR2Row> TableR2(string series) => Get<TableR2Row>(series, r => r.MortalityTable);

    public List<TableBRow> TableB() => Get<TableBRow>(null, null);

    public List<TableDRow> TableD() => Get<TableDRow>(null, null);

    public List<TableFRow> TableF() => Get<TableFRow>(null, null);

    public List<TableJRow> TableJ() => Get<TableJRow>(null, null);

    public List<TableKRow> TableK() => Get<TableKRow>(null, null);

    public List<MortalityTableRow> Mortality() => Get<MortalityTableRow>(null, null);

    /// <summary>lx values for one census year (1990 / 2000 / 2010).</summary>
    public List<MortalityTableRow> Mortality(int year)
    {
        var rows = Mortality().Where(r => r.Year == year).ToList();
        if (rows.Count == 0)
        {
            throw new ScenarioException($"MortalityTable has no rows for year {year}; published years: 1990, 2000, 2010.");
        }

        return rows;
    }

    public int CensusYear(string series) => FactorData.CensusYearOf(series);

    private List<TRow> Get<TRow>(string series, Func<TRow, int> censusYearOf) where TRow : class
    {
        if (series is not null)
        {
            FactorData.ValidateSeries(series);
        }

        var key = $"{typeof(TRow).Name}|{series ?? ""}";
        var lazy = _cache.GetOrAdd(key, _ => new Lazy<object>(() => Load<TRow>(series, censusYearOf)));
        return (List<TRow>)lazy.Value;
    }

    private List<TRow> Load<TRow>(string series, Func<TRow, int> censusYearOf) where TRow : class
    {
        using var db = new ActuarialDbContext(_connectionString);

        var rows = db.Set<TRow>().AsNoTracking().ToList();

        // the series filter is a delegate, so it runs in memory; the tables
        // are cached per (table, series) anyway
        return series is null ? rows : rows.Where(r => censusYearOf(r) == FactorData.CensusYearOf(series)).ToList();
    }
}