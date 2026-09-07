using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Logic.Loaders;
using DataProcessingApp.Logic.Savers;
using Xunit;

namespace DataProcessingApp.Tests
{
    public class TableSaverTests : IDisposable
    {
        private readonly string tempDir;

        public TableSaverTests()
        {
            tempDir = Path.Combine(Path.GetTempPath(), "dpa-tests", Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
        }

        public void Dispose()
        {
            Directory.Delete(tempDir, recursive: true);
        }

        [Fact]
        public void SaveToJsonFile_WritesRowsThatLoadBack()
        {
            var rows = new List<TableKRow>
            {
                new TableKRow { InterestRate = 0.2, Frequency = "Annual", AdjustmentFactor = 1.0 },
                new TableKRow { InterestRate = 10.4, Frequency = "Semi", AdjustmentFactor = 0.9999999999999991 }
            };
            var filename = Path.Combine(tempDir, "tablek.json");

            new TableSaver<TableKRow>().SaveToJsonFile(filename, rows);

            var loaded = new TableLoader<TableKRow>().LoadFromJson(filename);
            Assert.Equal(2, loaded.Count);
            Assert.Equal(0.9999999999999991, loaded[1].AdjustmentFactor);
        }

        [Fact]
        public void SaveToTextFile_WritesSpaceSeparatedValues()
        {
            var rows = new List<TableKRow>
            {
                new TableKRow { InterestRate = 0.2, Frequency = "Annual", AdjustmentFactor = 1.0 }
            };
            var filename = Path.Combine(tempDir, "tablek.txt");

            new TableSaver<TableKRow>().SaveToTextFile(filename, rows);

            Assert.Equal("0.2 Annual 1", File.ReadAllText(filename).Trim());
        }

        [Fact]
        public void SaveToExcel_WritesHeadersFromDbColumnsAndNumericCells()
        {
            var rows = new List<TableKRow>
            {
                new TableKRow { InterestRate = 0.2, Frequency = "Annual", AdjustmentFactor = 1.0 }
            };
            var filename = Path.Combine(tempDir, "tablek.xlsx");

            new TableSaver<TableKRow>().SaveToExcel("TableK", filename, rows);

            using (var workbook = new XLWorkbook(filename))
            {
                var worksheet = workbook.Worksheet(1);
                Assert.Equal("TableK", worksheet.Name);
                Assert.Equal("InterestRate", worksheet.Cell(1, 1).GetString());
                Assert.Equal("adjustmentFactor", worksheet.Cell(1, 3).GetString());
                Assert.Equal(XLDataType.Number, worksheet.Cell(2, 1).DataType);
                Assert.Equal(0.2, worksheet.Cell(2, 1).GetDouble());
                Assert.Equal("Annual", worksheet.Cell(2, 2).GetString());
            }
        }

        private string WriteFile(string name, string content)
        {
            var path = Path.Combine(tempDir, name);
            File.WriteAllText(path, content);
            return path;
        }
    }
}
