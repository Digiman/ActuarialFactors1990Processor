using System;
using System.Collections.Generic;
using System.IO;

namespace DataProcessingApp.Core.Helpers;

public enum TableType
{
    TableC,
    TableS,
    TableH,
    TableU1,
    TableU2,
    TableR2,
    TableB,
    TableD,
    TableF,
    TableJ,
    TableK,
    TableZ,
    MortalityTable
}

public enum DocumentType
{
    Excel,
    JSON,
    Text,
    XML
}

public static class FilesHelper
{
    private static readonly Dictionary<TableType, string> FileWithTables = new()
    {
        {TableType.TableC, "TableC-90CM-processed"},
        {TableType.TableS, "TableS-90CM-processed"},
        {TableType.TableH, "TableH-90CM-processed"},
        {TableType.TableU1, "TableU1-90CM-processed"},
        {TableType.TableR2, "TableR(2)-full-90CM"},
        {TableType.TableU2, "TableU(2)-full-90CM"},
        {TableType.TableB, "TableB"},
        {TableType.TableD, "TableD"},
        {TableType.TableF, "TableF"},
        {TableType.TableJ, "TableJ"},
        {TableType.TableK, "TableK"},
        {TableType.TableZ, "TableZ-2010CM-processed"},
        {TableType.MortalityTable, "MortalityTable"}
    };

    // Actuarial table series: 90CM (1990 census), 2000CM, 2010CM (current).
    public const string Series90CM = "90CM";
    public const string Series2000CM = "2000CM";
    public const string Series2010CM = "2010CM";

    public static readonly string[] TableU2Files =
    {
        "TableU(2)-p1-90CM",
        "TableU(2)-p2-90CM",
        "TableU(2)-p3-90CM",
        "TableU(2)-p4-90CM",
        "TableU(2)-p5-90CM"
    };

    public static readonly string[] TableR2Files =
    {
        "TableR(2)-p1-90CM",
        "TableR(2)-p2-90CM",
        "TableR(2)-p3-90CM",
        "TableR(2)-p4-90CM",
        "TableR(2)-p5-90CM"
    };

    public static string GenerateFilename(TableType tableType, DocumentType documentType)
    {
        return GenerateFilename(tableType, documentType, String.Empty);
    }

    public static string GenerateFilename(TableType tableType, DocumentType documentType, string series)
    {
        var name = GetFilenameByTableType(tableType);
        if (!string.IsNullOrEmpty(series))
        {
            name = name.Replace("-90CM", $"-{series}")
                .Replace("-2010CM", $"-{series}");
        }
        // XML files are root tables only and live in their own folder when configured;
        // JSON/Excel/text files live in BaseDataDir, series tables in BaseDataDir/<series>.
        var directory = documentType == DocumentType.XML
            ? AppHelper.XmlDataDir
            : string.IsNullOrEmpty(series) ? AppHelper.BaseDataDir : Path.Combine(AppHelper.BaseDataDir, series);
        return Path.Combine(directory, $"{name}.{GetFileExtension(documentType)}");
    }

    public static string GeneratePartFilename(string baseFilename)
    {
        return GeneratePartFilename(baseFilename, String.Empty);
    }

    public static string GeneratePartFilename(string baseFilename, string series)
    {
        if (!string.IsNullOrEmpty(series))
        {
            baseFilename = baseFilename.Replace("-90CM", $"-{series}");
        }
        var directory = string.IsNullOrEmpty(series) ? AppHelper.BaseDataDir : Path.Combine(AppHelper.BaseDataDir, series);
        return Path.Combine(directory, $"{baseFilename}-processed.json");
    }
    
    // Tables stored as JSON arrays (converted from official IRS spreadsheets / PDFs via Python);
    // all others are XML-based (SQL Server export format).
    private static readonly HashSet<TableType> XmlBasedTables =
    [
        TableType.TableB,
        TableType.TableD,
        TableType.TableF,
        TableType.TableJ,
        TableType.TableK,
        TableType.MortalityTable
    ];

    public static bool IsXmlBased(TableType tableType)
    {
        return XmlBasedTables.Contains(tableType);
    }

    public static string TableDisplayName(TableType tableType)
    {
        switch (tableType)
        {
            case TableType.TableC: return "TableC";
            case TableType.TableS: return "TableS";
            case TableType.TableH: return "TableH";
            case TableType.TableU1: return "TableU(1)";
            case TableType.TableU2: return "TableU(2)";
            case TableType.TableR2: return "TableR(2)";
            case TableType.TableB: return "TableB";
            case TableType.TableD: return "TableD";
            case TableType.TableF: return "TableF";
            case TableType.TableJ: return "TableJ";
            case TableType.TableK: return "TableK";
            case TableType.TableZ: return "TableZ";
            case TableType.MortalityTable: return "MortalityTable";
            default: return tableType.ToString();
        }
    }

    public static IReadOnlyList<string> PartFiles(TableType tableType)
    {
        return tableType switch
        {
            TableType.TableU2 => TableU2Files,
            TableType.TableR2 => TableR2Files,
            _ => (IReadOnlyList<string>)null
        };
    }

    private static string GetFileExtension(DocumentType documentType)
    {
        return documentType switch
        {
            DocumentType.Excel => "xlsx",
            DocumentType.JSON => "json",
            DocumentType.Text => "txt",
            DocumentType.XML => "xml",
            _ => String.Empty
        };
    }

    private static string GetFilenameByTableType(TableType tableType)
    {
        return FileWithTables[tableType];
    }
}