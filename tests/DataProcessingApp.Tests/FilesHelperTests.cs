using DataProcessingApp.Core.Helpers;
using System;
using System.IO;
using Xunit;

namespace DataProcessingApp.Tests;

public class FilesHelperTests
{
    [Fact]
    public void GenerateFilename_WithoutSeries_UsesBaseDataDirRoot()
    {
        var filename = FilesHelper.GenerateFilename(TableType.TableK, DocumentType.JSON);
        Assert.Equal(Path.Combine(AppHelper.BaseDataDir, "TableK.json"), filename);
    }

    [Fact]
    public void GenerateFilename_XmlDocument_UsesXmlDataDir()
    {
        var filename = FilesHelper.GenerateFilename(TableType.TableK, DocumentType.XML);
        Assert.Equal(Path.Combine(AppHelper.XmlDataDir, "TableK.xml"), filename);
    }

    [Fact]
    public void GenerateFilename_XmlDocument_IgnoresSeriesSubfolders()
    {
        var filename = FilesHelper.GenerateFilename(TableType.TableK, DocumentType.XML, FilesHelper.Series90CM);
        Assert.Equal(Path.Combine(AppHelper.XmlDataDir, "TableK.xml"), filename);
    }

    [Fact]
    public void GenerateFilename_WithSeries_UsesSeriesSubfolderAndSuffix()
    {
        var filename = FilesHelper.GenerateFilename(TableType.TableS, DocumentType.JSON, FilesHelper.Series2010CM);
        Assert.Equal(
            Path.Combine(AppHelper.BaseDataDir, "2010CM", "TableS-2010CM-processed.json"),
            filename);
    }

    [Fact]
    public void GenerateFilename_With2010Series_RenamesTableZCorrectly()
    {
        var filename = FilesHelper.GenerateFilename(TableType.TableZ, DocumentType.JSON, FilesHelper.Series2010CM);
        Assert.EndsWith("TableZ-2010CM-processed.json", filename);
    }

    [Theory]
    [InlineData(TableType.TableU2, "TableU(2)-p3-90CM")]
    [InlineData(TableType.TableR2, "TableR(2)-p5-90CM")]
    public void GeneratePartFilename_BuildsProcessedJsonName(TableType tableType, string baseFilename)
    {
        var filename = FilesHelper.GeneratePartFilename(baseFilename);
        Assert.Equal(
            Path.Combine(AppHelper.BaseDataDir, baseFilename + "-processed.json"),
            filename);
    }

    [Fact]
    public void GeneratePartFilename_WithSeries_ReplacesSeriesToken()
    {
        var filename = FilesHelper.GeneratePartFilename("TableU(2)-p1-90CM", FilesHelper.Series2010CM);
        Assert.EndsWith("TableU(2)-p1-2010CM-processed.json", filename);
    }

    [Theory]
    [InlineData(TableType.TableB, true)]
    [InlineData(TableType.TableK, true)]
    [InlineData(TableType.MortalityTable, true)]
    [InlineData(TableType.TableS, false)]
    [InlineData(TableType.TableR2, false)]
    public void IsXmlBased_DetectsSourceFormat(TableType tableType, bool expected)
    {
        Assert.Equal(expected, FilesHelper.IsXmlBased(tableType));
    }

    [Theory]
    [InlineData(TableType.TableU1, "TableU(1)")]
    [InlineData(TableType.TableR2, "TableR(2)")]
    [InlineData(TableType.TableK, "TableK")]
    public void TableDisplayName_ReturnsReadableName(TableType tableType, string expected)
    {
        Assert.Equal(expected, FilesHelper.TableDisplayName(tableType));
    }

    [Fact]
    public void PartFiles_ReturnsFivePartsForSplitTables()
    {
        Assert.Equal(5, FilesHelper.PartFiles(TableType.TableU2).Count);
        Assert.Equal(5, FilesHelper.PartFiles(TableType.TableR2).Count);
        Assert.Null(FilesHelper.PartFiles(TableType.TableS));
    }
}