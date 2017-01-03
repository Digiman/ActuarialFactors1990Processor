using System.Collections.Generic;
using System.IO;
using DataProcessingApp.ConsoleApp.Helpers;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Data;
using DataProcessingApp.Logic.Loaders;
using DataProcessingApp.Logic.Savers;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableU2Worker
    {
        public static void LoadTableData()
        {
            foreach (var file in FilesHelper.TableU2Files)
            {
                var filename = FilesHelper.GeneratePartFilename(file);
                LoadTablePartData(filename);
            }
        }

        public static void CombineTableParts()
        {
            var jsonFilename = FilesHelper.GenerateFilename(TableType.TableU2, DocumentType.JSON);

            if (!File.Exists(jsonFilename))
            {
                // read all parts with data
                var tableParts = new List<TableU2>();

                foreach (var file in FilesHelper.TableU2Files)
                {
                    var filename = FilesHelper.GeneratePartFilename(file);
                    tableParts.Add(LoadTablePartData(filename));
                }

                var combinedTable = CreateOneTable(tableParts);

                // save to JSON file
                var saver = new TableU2Saver();
                saver.SaveToJsonFile(jsonFilename, combinedTable);
            }
        }

        public static void ExportToExcel()
        {
            // 1. Load data from JSON file.
            var filename = FilesHelper.GenerateFilename(TableType.TableU2, DocumentType.JSON);

            var loader = new TableU2Loader();
            var result = loader.LoadFromJSON(filename);

            // 2. Save data to Excel document.
            var excelFilename = FilesHelper.GenerateFilename(TableType.TableU2, DocumentType.Excel);

            var saver = new TableU2Saver();
            saver.SaveToExcel(result, excelFilename);
        }

        public static void SaveToTextFileFile()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableU2, DocumentType.JSON);

            var loader = new TableU2Loader();
            var result = loader.LoadFromJSON(filename);

            // save
            var textFilename = FilesHelper.GenerateFilename(TableType.TableU2, DocumentType.Text);

            var saver = new TableU2Saver();
            saver.SaveToTextFile(textFilename, result);
        }

        public static void SaveToDatabase()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableU2, DocumentType.JSON);

            var loader = new TableU2Loader();
            var result = loader.LoadFromJSON(filename);

            // save to database
            var repository = DataFactory.Instance.GetTableU2Repository(AppHelper.DatabaseConnectionString);
            repository.InsertTableData(result);
        }

        #region Helpers.

        private static TableU2 CreateOneTable(List<TableU2> tableParts)
        {
            var table = new TableU2();

            foreach (var tablePart in tableParts)
            {
                table.Rows.AddRange(tablePart.Rows);
            }

            return table;
        }

        private static TableU2 LoadTablePartData(string filename)
        {
            var loader = new TableU2Loader();
            var result = loader.LoadFromJSON(filename);

            return result;
        }

        #endregion
    }
}
