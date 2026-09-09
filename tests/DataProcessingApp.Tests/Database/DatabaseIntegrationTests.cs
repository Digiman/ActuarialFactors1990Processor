using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Data;
using DataProcessingApp.Data.Repositories;
using DataProcessingApp.Logic.Loaders;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Xunit;

namespace DataProcessingApp.Tests.Database;

/// <summary>
/// End-to-end tests of the database path (SqlBulkCopy + stored procedures)
/// against a real SQL Server deployed from the db/ project files. Tests are
/// skipped when no SQL Server is reachable.
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
    public void StoredProcedures_ReadBackBulkInsertedData()
    {
        Skip.IfNot(_fixture.Available, _fixture.UnavailableReason);

        var rows = LoadTableSRows();
        new TableRepository<TableSRow>(_fixture.ConnectionString, TableType.TableS).InsertTableData(rows);

        var expected = rows.Single(row => row.Age == 60 && row.InterestRate == 2.2);
        var pvAnnuity = ExecuteScalarProcedure("[dbo].[GetPresentValueAnnuityFromTableS]", parameter =>
        {
            parameter.Add("@Mortality", SqlDbType.Int).Value = 1990;
            parameter.Add("@Age", SqlDbType.Int).Value = 60;
            parameter.Add("@Rate", SqlDbType.Float).Value = 2.2;
        });
        Assert.Equal(expected.PvAnnuity, (double)pvAnnuity, 10);
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public void GetLxFrom2010_ReturnsMortalityTableValue()
    {
        Skip.IfNot(_fixture.Available, _fixture.UnavailableReason);

        var rows = new TableLoader<MortalityTableRow>().LoadFromJson(
            Path.Combine(_fixture.RepositoryRoot, "JSONFiles", "MortalityTable.json"));
        new TableRepository<MortalityTableRow>(_fixture.ConnectionString, TableType.MortalityTable)
            .InsertTableData(rows);

        var expected = rows.Single(row => row.Year == 2010 && row.Age == 60).Lx;
        var lx = ExecuteScalarProcedure("[dbo].[GetLxFrom2010]", parameter =>
        {
            parameter.Add("@Age", SqlDbType.Int).Value = 60;
        });
        Assert.Equal(expected, (double)lx, 10);
    }

    private List<TableSRow> LoadTableSRows()
    {
        return new TableLoader<TableSRow>().LoadFromJson(Path.Combine(
            _fixture.RepositoryRoot, "JSONFiles", "90CM", "TableS-90CM-processed.json"));
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

    private object ExecuteScalarProcedure(string procedureName, Action<SqlParameterCollection> addParameters)
    {
        using (var connection = new SqlConnection(_fixture.ConnectionString))
        {
            connection.Open();
            using (var command = new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure })
            {
                addParameters(command.Parameters);
                return command.ExecuteScalar();
            }
        }
    }
}