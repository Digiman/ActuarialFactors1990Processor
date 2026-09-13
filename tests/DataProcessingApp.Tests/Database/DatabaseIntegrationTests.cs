using DataProcessingApp.Calculator;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Data.Repositories;
using DataProcessingApp.DataAccess;
using DataProcessingApp.Logic.Loaders;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace DataProcessingApp.Tests.Database;

/// <summary>
/// End-to-end tests of the database path (SqlBulkCopy seeder + EF Core
/// reads) against a real SQL Server migrated from DataProcessingApp.DataAccess.
/// Tests are skipped when no SQL Server is reachable.
/// </summary>
[Collection("sqlserver")]
public class DatabaseIntegrationTests
{
    private readonly SqlServerFixture _fixture;

    public DatabaseIntegrationTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public void TableS_BulkInsert_IsIdempotent()
    {
        Skip.IfNot(_fixture.Available, _fixture.UnavailableReason);

        var rows = LoadTableSRows();
        var repository = new TableRepository<TableSRow>(_fixture.ConnectionString, TableType.TableS);

        var inserted = repository.InsertTableData(rows);
        Assert.Equal(rows.Count, inserted);
        Assert.Equal(rows.Count, CountRows("[dbo].[tblS]"));

        repository.InsertTableData(rows);
        Assert.Equal(rows.Count, CountRows("[dbo].[tblS]"));
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public void SeriesBackedTable_BulkInsert_ClearsOnlyItsOwnCensusYear()
    {
        Skip.IfNot(_fixture.Available, _fixture.UnavailableReason);

        var repository = new TableRepository<TableSRow>(_fixture.ConnectionString, TableType.TableS);
        var year1990 = LoadTableSRows();
        var year2000 = new TableLoader<TableSRow>().LoadFromJson(TableSJson(FilesHelper.Series2000CM));

        repository.InsertTableData(year1990, 1990);
        repository.InsertTableData(year2000, 2000);
        Assert.Equal(22000, CountRows("[dbo].[tblS]"));

        // reseeding one series must not wipe the other
        repository.InsertTableData(year1990, 1990);
        Assert.Equal(22000, CountRows("[dbo].[tblS]"));
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public void FailedBulkCopy_RollsBackAndKeepsPreviousData()
    {
        Skip.IfNot(_fixture.Available, _fixture.UnavailableReason);

        var repository = new TableRepository<TableFRow>(_fixture.ConnectionString, TableType.TableF);
        var valid = new List<TableFRow>
        {
            new() { InterestRate = 4.2, Frequency = "Annual", Months = 0, AdjustmentFactor = 1.0 }
        };
        repository.InsertTableData(valid);

        var broken = new List<TableFRow>
        {
            new() { InterestRate = 4.2, Frequency = null, Months = 0, AdjustmentFactor = 1.0 }
        };
        Assert.ThrowsAny<Exception>(() => repository.InsertTableData(broken));

        Assert.Equal(1, CountRows("[dbo].[tblF]"));
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public void EF_ReadsBackBulkInsertedData()
    {
        Skip.IfNot(_fixture.Available, _fixture.UnavailableReason);

        var rows = LoadTableSRows();
        new TableRepository<TableSRow>(_fixture.ConnectionString, TableType.TableS).InsertTableData(rows, 1990);

        using var db = new ActuarialDbContext(_fixture.ConnectionString);
        var expected = rows.Single(row => row.Age == 60 && row.InterestRate == 2.2);
        var actual = db.Set<TableSRow>().AsNoTracking()
            .Where(r => r.MortalityTable == 1990)
            .Single(r => r.Age == 60 && r.InterestRate == 2.2);

        Assert.Equal(expected.PvAnnuity, actual.PvAnnuity, 10);
        Assert.Equal(expected.PvLifeEstate, actual.PvLifeEstate, 10);
        Assert.Equal(expected.PvReminderInterest, actual.PvReminderInterest, 10);
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public void DbFactorData_LoadsEverySeriesFromTheDatabase()
    {
        Skip.IfNot(_fixture.Available, _fixture.UnavailableReason);

        foreach (var series in FactorData.Series)
        {
            var rows = new TableLoader<TableSRow>().LoadFromJson(TableSJson(series));
            new TableRepository<TableSRow>(_fixture.ConnectionString, TableType.TableS)
                .InsertTableData(rows, FactorData.CensusYearOf(series));
        }

        var mortality = new TableLoader<MortalityTableRow>().LoadFromJson(
            Path.Combine(_fixture.RepositoryRoot, "JSONFiles", "MortalityTable.json"));
        new TableRepository<MortalityTableRow>(_fixture.ConnectionString, TableType.MortalityTable)
            .InsertTableData(mortality);

        var data = new DbFactorData(_fixture.ConnectionString);
        foreach (var series in FactorData.Series)
        {
            var rows = data.TableS(series);
            Assert.Equal(11000, rows.Count);
            Assert.All(rows, r => Assert.Equal(FactorData.CensusYearOf(series), r.MortalityTable));
        }

        Assert.Equal(444, data.Mortality().Count);
        Assert.DoesNotContain(data.TableS("90CM"), r => r.MortalityTable != 1990);
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public void DbFactorData_SeriesValidationMatchesFileSource()
    {
        Skip.IfNot(_fixture.Available, _fixture.UnavailableReason);

        var data = new DbFactorData(_fixture.ConnectionString);
        Assert.Throws<ScenarioException>(() => data.TableS("1980CM"));
    }

    private List<TableSRow> LoadTableSRows()
    {
        return new TableLoader<TableSRow>().LoadFromJson(
            TableSJson(FilesHelper.Series90CM));
    }

    private string TableSJson(string series)
    {
        return Path.Combine(_fixture.RepositoryRoot, "JSONFiles", series, $"TableS-{series}-processed.json");
    }

    private int CountRows(string tableName)
    {
        using (var connection = new SqlConnection(_fixture.ConnectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT COUNT(*) FROM {tableName}";
                return (int)command.ExecuteScalar();
            }
        }
    }
}