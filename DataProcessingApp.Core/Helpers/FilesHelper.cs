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
            {TableType.TableC, "tabula-TableC-1990-processed"},
            {TableType.TableS, "tabula-TableS-1990-processed"},
            {TableType.TableH, "tabula-TableH-1990-processed"},
            {TableType.TableU1, "tabula-TableU1-1990-processed"},
            {TableType.TableR2, "tabula-TableR(2)-full-1990"},
            {TableType.TableU2, "tabula-TableU(2)-full-1990"},
            {TableType.TableB, "TableB-2000"},
            {TableType.TableD, "TableD-2000"},
            {TableType.TableF, "TableF-2000"},
            {TableType.TableJ, "TableJ-2000"},
            {TableType.TableK, "TableK-2000"},
            {TableType.MortalityTable, "MortalityTable-2000"}
        };

        public static readonly string[] TableU2Files =
        {
            "tabula-TableU(2)-p1-1990",
            "tabula-TableU(2)-p2-1990",
            "tabula-TableU(2)-p3-1990",
            "tabula-TableU(2)-p4-1990",
            "tabula-TableU(2)-p5-1990"
        };

        public static readonly string[] TableR2Files =
        {
            "tabula-TableR(2)-p1-1990",
            "tabula-TableR(2)-p2-1990",
            "tabula-TableR(2)-p3-1990",
            "tabula-TableR(2)-p4-1990",
            "tabula-TableR(2)-p5-1990"
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
