using System.Collections.Generic;
using System.IO;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Data.Repositories;
using DataProcessingApp.Logic.Loaders;
using DataProcessingApp.Logic.Savers;

namespace DataProcessingApp.ConsoleApp.Workers
{
    /// <summary>
    /// Generic worker that orchestrates load/save operations for a single actuarial table.
    /// </summary>
    public class TableWorker<TRow> where TRow : new()
    {
        private readonly TableType tableType;
        private readonly string series;

        public TableWorker(TableType tableType, string series = "")
        {
            this.tableType = tableType;
            this.series = series;
        }

        /// <summary>
        /// Loads table data without saving it anywhere (smoke check of the source files).
        /// </summary>
        public void LoadTableData()
        {
            var loader = new TableLoader<TRow>();
            var parts = FilesHelper.PartFiles(tableType);
            if (parts != null)
            {
                foreach (var part in parts)
                {
                    loader.LoadFromJson(FilesHelper.GeneratePartFilename(part, series));
                }
            }
            else
            {
                LoadTable(loader);
            }
        }

        /// <summary>
        /// Loads table data from its native format and saves it as a JSON file.
        /// </summary>
        public void SaveToJsonFile()
        {
            var rows = LoadTable(new TableLoader<TRow>());

            var resultFilename = FilesHelper.GenerateFilename(tableType, DocumentType.JSON, series);
            new TableSaver<TRow>().SaveToJsonFile(resultFilename, rows);
        }

        /// <summary>
        /// Combines per-part JSON files into one combined JSON file
        /// (only for tables that are split into parts; no-op otherwise).
        /// </summary>
        public void CombineTableParts()
        {
            var parts = FilesHelper.PartFiles(tableType);
            if (parts == null)
            {
                return;
            }

            var jsonFilename = FilesHelper.GenerateFilename(tableType, DocumentType.JSON, series);
            if (File.Exists(jsonFilename))
            {
                return;
            }

            var loader = new TableLoader<TRow>();
            var combined = new List<TRow>();
            foreach (var part in parts)
            {
                combined.AddRange(loader.LoadFromJson(FilesHelper.GeneratePartFilename(part, series)));
            }

            new TableSaver<TRow>().SaveToJsonFile(jsonFilename, combined);
        }

        /// <summary>
        /// Loads table data and saves it as an Excel document.
        /// </summary>
        public void ExportToExcel()
        {
            var rows = LoadTable(new TableLoader<TRow>());

            var excelFilename = FilesHelper.GenerateFilename(tableType, DocumentType.Excel, series);
            new TableSaver<TRow>().SaveToExcel(FilesHelper.TableDisplayName(tableType), excelFilename, rows);
        }

        /// <summary>
        /// Loads table data and saves it as a plain text file.
        /// </summary>
        public void SaveToTextFile()
        {
            var rows = LoadTable(new TableLoader<TRow>());

            var textFilename = FilesHelper.GenerateFilename(tableType, DocumentType.Text, series);
            new TableSaver<TRow>().SaveToTextFile(textFilename, rows);
        }

        /// <summary>
        /// Loads table data and bulk-inserts it into the SQL Server destination table.
        /// </summary>
        public void SaveToDatabase()
        {
            var rows = LoadTable(new TableLoader<TRow>());

            var repository = new TableRepository<TRow>(AppHelper.DatabaseConnectionString, tableType);
            repository.InsertTableData(rows);
        }

        private List<TRow> LoadTable(TableLoader<TRow> loader)
        {
            return FilesHelper.IsXmlBased(tableType)
                ? loader.LoadFromXml(FilesHelper.GenerateFilename(tableType, DocumentType.XML, series))
                : loader.LoadFromJson(FilesHelper.GenerateFilename(tableType, DocumentType.JSON, series));
        }
    }
}
