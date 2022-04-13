using System;
using System.Collections.Generic;

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
            {TableType.TableC, "TableC-1990-processed"},
            {TableType.TableS, "TableS-1990-processed"},
            {TableType.TableH, "TableH-1990-processed"},
            {TableType.TableU1, "TableU1-1990-processed"},
            {TableType.TableR2, "TableR(2)-full-1990"},
            {TableType.TableU2, "TableU(2)-full-1990"},
            {TableType.TableB, "TableB"},
            {TableType.TableD, "TableD"},
            {TableType.TableF, "TableF"},
            {TableType.TableJ, "TableJ"},
            {TableType.TableK, "TableK"},
            {TableType.MortalityTable, "MortalityTable"}
        };

        public static readonly string[] TableU2Files =
        {
            "TableU(2)-p1-1990",
            "TableU(2)-p2-1990",
            "TableU(2)-p3-1990",
            "TableU(2)-p4-1990",
            "TableU(2)-p5-1990"
        };

        public static readonly string[] TableR2Files =
        {
            "TableR(2)-p1-1990",
            "TableR(2)-p2-1990",
            "TableR(2)-p3-1990",
            "TableR(2)-p4-1990",
            "TableR(2)-p5-1990"
        };

        public static string GenerateFilename(TableType tableType, DocumentType documentType)
        {
            return String.Format("{0}\\{1}.{2}", AppHelper.BaseDataDir, GetFilenameByTableType(tableType), GetFileExtension(documentType));
        }

        public static string GeneratePartFilename(string baseFilename)
        {
            return String.Format("{0}\\{1}-processed.json", AppHelper.BaseDataDir, baseFilename);
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
