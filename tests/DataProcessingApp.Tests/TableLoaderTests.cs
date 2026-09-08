using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Logic.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace DataProcessingApp.Tests;

public class TableLoaderTests : IDisposable
{
    private readonly string tempDir;

    public TableLoaderTests()
    {
        tempDir = Path.Combine(Path.GetTempPath(), "dpa-tests", Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
    }

    public void Dispose()
    {
        Directory.Delete(tempDir, recursive: true);
    }

    [Fact]
    public void LoadFromJson_ReadsRowsAndAppliesRounding()
    {
        // rate 2.26 must be rounded to 1 decimal (Round attribute on TableCRow.Rate)
        var json = "[{\"mortalityTable\":1990,\"rate\":2.26,\"age\":\"5\"," +
                   "\"remainderFactor\":\".01463\",\"rFactor\":\"13619.32\",\"dFactor\":\"100000.0\"}]";
        var filename = WriteFile("tablec.json", json);

        var rows = new TableLoader<TableCRow>().LoadFromJson(filename);

        var row = Assert.Single(rows);
        Assert.Equal(1990, row.MortalityTable);
        Assert.Equal(2.3, row.Rate);
        Assert.Equal(5, row.Age);
        Assert.Equal(0.01463, row.RemainderFactor);
        Assert.Equal(13619.32, row.RFactor);
        Assert.Equal(100000.0, row.DFactor);
    }

    [Fact]
    public void LoadFromJson_ReadsMultipleRows()
    {
        var json = "[{\"InterestRate\":2.2,\"Frequency\":\"Annual\",\"adjustmentFactor\":1.0}," +
                   "{\"InterestRate\":4.2,\"Frequency\":\"Semi\",\"adjustmentFactor\":0.999778}]";
        var filename = WriteFile("tablek.json", json);

        var rows = new List<TableKRow>(new TableLoader<TableKRow>().LoadFromJson(filename));

        Assert.Equal(2, rows.Count);
        Assert.Equal("Semi", rows[1].Frequency);
        Assert.Equal(0.999778, rows[1].AdjustmentFactor);
    }

    [Fact]
    public void LoadFromXml_ParsesAttributesCaseInsensitively()
    {
        // SQL Server export format uses mixed-case attribute names
        var xml = "<dbo.tblK>" +
                  "<dbo.tblK InterestRate=\"0.2\" Frequency=\"Annual\" adjustmentFactor=\"1.0\" />" +
                  "<dbo.tblK InterestRate=\"10.4\" Frequency=\"Semi\" adjustmentFactor=\"0.9999999999999991\" />" +
                  "</dbo.tblK>";
        var filename = WriteFile("tablek.xml", xml);

        var rows = new TableLoader<TableKRow>().LoadFromXml(filename);

        Assert.Equal(2, rows.Count);
        Assert.Equal(0.2, rows[0].InterestRate);
        Assert.Equal("Annual", rows[0].Frequency);
        Assert.Equal(1.0, rows[0].AdjustmentFactor);
        Assert.Equal(10.4, rows[1].InterestRate);
        Assert.Equal("Semi", rows[1].Frequency);
    }

    [Fact]
    public void LoadFromXml_ParsesIntegerColumns()
    {
        var xml = "<dbo.tblMortality>" +
                  "<dbo.tblMortality Year=\"1990\" Age=\"0\" lx=\"100000\" />" +
                  // 2010CM series contains fractional lx values
                  "<dbo.tblMortality Year=\"2010\" Age=\"1\" lx=\"99382.28\" />" +
                  "</dbo.tblMortality>";
        var filename = WriteFile("mortality.xml", xml);

        var rows = new TableLoader<MortalityTableRow>().LoadFromXml(filename);

        Assert.Equal(2, rows.Count);
        Assert.Equal(1990, rows[0].Year);
        Assert.Equal(0, rows[0].Age);
        Assert.Equal(100000, rows[0].Lx);
        Assert.Equal(99382.28, rows[1].Lx);
    }

    [Fact]
    public void LoadFromXml_MissingAttribute_ThrowsHelpfulError()
    {
        var xml = "<dbo.tblK><dbo.tblK InterestRate=\"0.2\" /></dbo.tblK>";
        var filename = WriteFile("bad.xml", xml);

        var exception = Assert.Throws<FormatException>(
            () => new TableLoader<TableKRow>().LoadFromXml(filename));
        Assert.Contains("Frequency", exception.Message);
    }

    private string WriteFile(string name, string content)
    {
        var path = Path.Combine(tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }
}