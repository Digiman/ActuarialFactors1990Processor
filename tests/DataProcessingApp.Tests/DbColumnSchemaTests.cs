using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using Xunit;

namespace DataProcessingApp.Tests
{
    /// <summary>
    /// Guards the [DbColumn] names on row classes against the actual SQL Server
    /// schema in db/DataProcessingApp.Database. Bulk insert maps columns by name,
    /// so a mismatch here fails at insert time; this test fails at build time.
    /// </summary>
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
            (typeof(TableU2Row), "tblU2")
        };

        public static IEnumerable<object[]> Tables()
        {
            return Mappings.Select(m => new object[] { m.RowType, m.Table });
        }

        [Theory]
        [MemberData(nameof(Tables))]
        public void DbColumnNames_MatchSqlServerSchema(Type rowType, string tableName)
        {
            var schemaColumns = ParseSchemaColumns(tableName);
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

        [Fact]
        public void EveryDbBackedRowType_IsCoveredByTheTheory()
        {
            // fail loudly when a new tbl*.sql is added without a matching row type
            var schemaTables = Directory.GetFiles(SchemaDirectory(), "tbl*.sql")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(x => x)
                .ToList();
            var coveredTables = Mappings.Select(m => m.Table).OrderBy(x => x).ToList();

            Assert.Equal(schemaTables, coveredTables);
        }

        private static string SchemaDirectory()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "DataProcessingApp.sln")))
            {
                dir = dir.Parent;
            }

            Assert.NotNull(dir);
            return Path.Combine(dir.FullName, "db", "DataProcessingApp.Database", "dbo", "tables");
        }

        private static List<string> ParseSchemaColumns(string tableName)
        {
            var sql = File.ReadAllText(Path.Combine(SchemaDirectory(), tableName + ".sql"));
            var matches = Regex.Matches(sql, @"^\s*\[(\w+)\]\s*\[\w+\]", RegexOptions.Multiline);
            return matches.Select(m => m.Groups[1].Value).ToList();
        }
    }
}
