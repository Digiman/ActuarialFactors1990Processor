using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Data;
using DataProcessingApp.Logic.Loaders;
using DataProcessingApp.Logic.Savers;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableKWorker
    {
        public static void LoadTableData()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableK, DocumentType.XML);

            var loader = new TableKLoader();
            loader.LoadFromXML(filename);
        }

        public static void SaveToDatabase()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableK, DocumentType.XML);

            var loader = new TableKLoader();
            var result = loader.LoadFromXML(filename);

            // save to database
            var repository = DataFactory.Instance.GetTableKRepository(AppHelper.DatabaseConnectionString);
            repository.InsertTableData(result);
        }

        public static void SaveToJsonFile()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableK, DocumentType.XML);

            var loader = new TableKLoader();
            var result = loader.LoadFromXML(filename);

            // save to JSON file
            var resultFilename = FilesHelper.GenerateFilename(TableType.TableK, DocumentType.JSON);

            var saver = new TableKSaver();
            saver.SaveToJsonFile(resultFilename, result);
        }

        public static void ExportToExcel()
        {
            // 1. Load data from JSON file.
            var filename = FilesHelper.GenerateFilename(TableType.TableK, DocumentType.XML);

            var loader = new TableKLoader();
            var result = loader.LoadFromXML(filename);

            // 2. Save data to Excel document.
            var excelFilename = FilesHelper.GenerateFilename(TableType.TableK, DocumentType.Excel);

            var saver = new TableKSaver();
            saver.SaveToExcel(result, excelFilename);
        }
    }
}
