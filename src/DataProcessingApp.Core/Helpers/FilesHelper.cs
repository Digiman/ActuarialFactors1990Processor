using System;
using System.Collections.Generic;
using System.IO;

namespace DataProcessingApp.Core.Helpers
{
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
        private static readonly Dictionary<TableType, string> FileWithTables = new Dictionary<TableType, string>
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
            if (!String.IsNullOrEmpty(series))
            {
                name = name.Replace("-90CM", String.Format("-{0}", series))
                           .Replace("-2010CM", String.Format("-{0}", series));
            }
            var directory = String.IsNullOrEmpty(series) ? AppHelper.BaseDataDir : Path.Combine(AppHelper.BaseDataDir, series);
            return Path.Combine(directory, String.Format("{0}.{1}", name, GetFileExtension(documentType)));
        }

        public static string GeneratePartFilename(string baseFilename)
        {
            return GeneratePartFilename(baseFilename, String.Empty);
        }

        public static string GeneratePartFilename(string baseFilename, string series)
        {
            if (!String.IsNullOrEmpty(series))
            {
                baseFilename = baseFilename.Replace("-90CM", String.Format("-{0}", series));
            }
            var directory = String.IsNullOrEmpty(series) ? AppHelper.BaseDataDir : Path.Combine(AppHelper.BaseDataDir, series);
            return Path.Combine(directory, String.Format("{0}-processed.json", baseFilename));
        }

        private static string GetFileExtension(DocumentType documentType)
        {
            switch (documentType)
            {
                case DocumentType.Excel:
                    return "xlsx";
                case DocumentType.JSON:
                    return "json";
                case DocumentType.Text:
                    return "txt";
                case DocumentType.XML:
                    return "xml";
                default:
                    return String.Empty;
            }
        }

        private static string GetFilenameByTableType(TableType tableType)
        {
            return FileWithTables[tableType];
        }
    }
}
