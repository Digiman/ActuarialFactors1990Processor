using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Data;
using DataProcessingApp.Logic.Loaders;
using DataProcessingApp.Logic.Savers;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableSWorker
    {
        public static void LoadTableData()
        {
            var filename = FilesHelper.GenerateFilename(TableType.TableS, DocumentType.JSON);

            var loader = new TableSLoader();
            loader.LoadFromJSON(filename);
        }

        public static void ExportToExcel()
        {
            // 1. Load data from JSON file.
            var filename = FilesHelper.GenerateFilename(TableType.TableS, DocumentType.JSON);

            var loader = new TableSLoader();
            var result = loader.LoadFromJSON(filename);

            // 2. Save data to Excel document.
            var excelFilename = FilesHelper.GenerateFilename(TableType.TableS, DocumentType.Excel);

            var saver = new TableSSaver();
            saver.SaveToExcel(result, excelFilename);
        }

        public static void SaveToTextFileFile()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableS, DocumentType.JSON);

            var loader = new TableSLoader();
            var result = loader.LoadFromJSON(filename);

            // save
            var textFilename = FilesHelper.GenerateFilename(TableType.TableS, DocumentType.Text);

            var saver = new TableSSaver();
            saver.SaveToTextFile(textFilename, result);
        }

        public static void SaveToDatabase()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableS, DocumentType.JSON);

            var loader = new TableSLoader();
            var result = loader.LoadFromJSON(filename);

            // save to database
            var repository = DataFactory.Instance.GetTableSRepository(AppHelper.DatabaseConnectionString);
            repository.InsertTableData(result);
        }
    }
}
