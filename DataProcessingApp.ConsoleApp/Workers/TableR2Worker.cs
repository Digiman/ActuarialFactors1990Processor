using System.Collections.Generic;
using System.IO;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Logic.Loaders;
using DataProcessingApp.Logic.Savers;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableR2Worker
    {
        public static void LoadTableData()
        {
            foreach (var file in FilesHelper.TableR2Files)
            {
                var filename = FilesHelper.GeneratePartFilename(file);
                LoadTablePartData(filename);
            }
        }

        public static void CombineTableParts()
        {
            var jsonFilename = FilesHelper.GenerateFilename(TableType.TableR2, DocumentType.JSON);

            if (!File.Exists(jsonFilename))
            {
                // read all parts with data
                var tableParts = new List<TableR2>();

                foreach (var file in FilesHelper.TableR2Files)
                {
                    var filename = FilesHelper.GeneratePartFilename(file);
                    tableParts.Add(LoadTablePartData(filename));
                }

                var combinedTable = CreateOneTable(tableParts);

                // save to JSON file
                var saver = new TableR2Saver();
                saver.SaveToJsonFile(jsonFilename, combinedTable);
            }
        }

        public static void ExportToExcel()
        {
            // 1. Load data from JSON file.
            var filename = FilesHelper.GenerateFilename(TableType.TableR2, DocumentType.JSON);

            var loader = new TableR2Loader();
            var result = loader.LoadFromJSON(filename);

            // 2. Save data to Excel document.
            var excelFilename = FilesHelper.GenerateFilename(TableType.TableR2, DocumentType.Excel);

            var saver = new TableR2Saver();
            saver.SaveToExcel(result, excelFilename);
        }

        public static void SaveToTextFileFile()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableR2, DocumentType.JSON);

            var loader = new TableR2Loader();
            var result = loader.LoadFromJSON(filename);

            // save
            var textFilename = FilesHelper.GenerateFilename(TableType.TableR2, DocumentType.Text);

            var saver = new TableR2Saver();
            saver.SaveToTextFile(textFilename, result);
        }

        private static TableR2 CreateOneTable(List<TableR2> tableParts)
        {
            var table = new TableR2();

            foreach (var tablePart in tableParts)
            {
                table.Rows.AddRange(tablePart.Rows);
            }

            return table;
        }

        private static TableR2 LoadTablePartData(string filename)
        {
            var loader = new TableR2Loader();
            var result = loader.LoadFromJSON(filename);

            return result;
        }
    }
}
