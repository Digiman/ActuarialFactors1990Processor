using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DataProcessingApp.Tests.Database;

/// <summary>
/// Guards the [DbColumn] names on row classes against the live SQL Server
/// schema deployed by the EF migrations. Bulk insert maps columns by exact
/// name, so a mismatch here fails at insert time; this test fails in CI the
/// moment the EF model and the row classes drift apart.
/// </summary>
[Collection("sqlserver")]
public class DbColumnSchemaTests
{
    private static readonly (Type RowType, string Table)[] Mappings =
    {
        (typeof(TableBRow), "tblB"),
        (typeof(TableCRow), "tblC"),
        (typeof(TableDRow), "tblD"),
        (typeof(TableFRow), "tblF"),
        (typeof(TableHRow), "tblH"),
        (typeof(TableJRow), "tblJ"),
        (typeof(TableKRow), "tblK"),
        (typeof(MortalityTableRow), "tblMortality"),
        (typeof(TableR2Row), "tblR2"),
        (typeof(TableSRow), "tblS"),
        (typeof(TableU1Row), "tblU1"),
        (typeof(TableU2Row), "tblU2"),
        (typeof(TableZRow), "tblZ")
    };

    public static IEnumerable<object[]> Tables()
    {
        return Mappings.Select(m => new object[] { m.RowType, m.Table });
    }

    private readonly SqlServerFixture _fixture;

    public DbColumnSchemaTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableTheory]
    [MemberData(nameof(Tables))]
    [Trait("Category", "Integration")]
    public void DbColumnNames_MatchSqlServerSchema(Type rowType, string tableName)
    {
        Skip.IfNot(_fixture.Available, _fixture.UnavailableReason);

        var schemaColumns = QuerySchemaColumns(tableName);
        Assert.NotEmpty(schemaColumns);

        var codeColumns = rowType.GetProperties()
            .Select(p =>
            {
                var attribute = (DbColumnAttribute)Attribute.GetCustomAttribute(p, typeof(DbColumnAttribute));
                return attribute != null ? attribute.Name : p.Name;
            })
            .ToList();

        // bulk copy maps by column name, so only the name set must match
        Assert.Equal(
            string.Join(",", schemaColumns.OrderBy(x => x)),
            string.Join(",", codeColumns.OrderBy(x => x)));
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public void MigratedSchema_ContainsAllActuarialTables()
    {
        Skip.IfNot(_fixture.Available, _fixture.UnavailableReason);

        foreach (var (rowType, table) in Mappings)
        {
            Assert.NotEmpty(QuerySchemaColumns(table));
        }
    }

    private List<string> QuerySchemaColumns(string tableName)
    {
        using (var connection = new SqlConnection(_fixture.ConnectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table";
                command.Parameters.AddWithValue("@table", tableName);

                var columns = new List<string>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add((string)reader["COLUMN_NAME"]);
                    }
                }
                return columns;
            }
        }
    }
}