using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Data;
using DataProcessingApp.Logic.Loaders;
using DataProcessingApp.Logic.Savers;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableBWorker
    {
        public static void LoadTableData()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableB, DocumentType.XML);

            var loader = new TableBLoader();
            loader.LoadFromXML(filename);
        }

        public static void SaveToDatabase()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableB, DocumentType.XML);

            var loader = new TableBLoader();
            var result = loader.LoadFromXML(filename);

            // save to database
            var repository = DataFactory.Instance.GetTableBRepository(AppHelper.DatabaseConnectionString);
            repository.InsertTableData(result);
        }

        public static void SaveToJsonFile()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableB, DocumentType.XML);

            var loader = new TableBLoader();
            var result = loader.LoadFromXML(filename);

            // save to JSON file
            var resultFilename = FilesHelper.GenerateFilename(TableType.TableB, DocumentType.JSON);

            var saver = new TableBSaver();
            saver.SaveToJsonFile(resultFilename, result);
        }

        public static void ExportToExcel()
        {
            // 1. Load data from JSON file.
            var filename = FilesHelper.GenerateFilename(TableType.TableB, DocumentType.XML);

            var loader = new TableBLoader();
            var result = loader.LoadFromXML(filename);

            // 2. Save data to Excel document.
            var excelFilename = FilesHelper.GenerateFilename(TableType.TableB, DocumentType.Excel);

            var saver = new TableBSaver();
            saver.SaveToExcel(result, excelFilename);
        }
    }
}
