using DataProcessingApp.ConsoleApp.Helpers;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Data;
using DataProcessingApp.Logic.Loaders;
using DataProcessingApp.Logic.Savers;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableU1Worker
    {
        public static void LoadTableData()
        {
            var filename = FilesHelper.GenerateFilename(TableType.TableU1, DocumentType.JSON);

            var loader = new TableU1Loader();
            loader.LoadFromJSON(filename);
        }

        public static void ExportToExcel()
        {
            // 1. Load data from JSON file.
            var filename = FilesHelper.GenerateFilename(TableType.TableU1, DocumentType.JSON);

            var loader = new TableU1Loader();
            var result = loader.LoadFromJSON(filename);

            // 2. Save data to Excel document.
            var excelFilename = FilesHelper.GenerateFilename(TableType.TableU1, DocumentType.Excel);

            var saver = new TableU1Saver();
            saver.SaveToExcel(result, excelFilename);
        }

        public static void SaveToTextFileFile()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableU1, DocumentType.JSON);

            var loader = new TableU1Loader();
            var result = loader.LoadFromJSON(filename);

            // save
            var textFilename = FilesHelper.GenerateFilename(TableType.TableU1, DocumentType.Text);

            var saver = new TableU1Saver();
            saver.SaveToTextFile(textFilename, result);
        }

        public static void SaveToDatabase()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableU1, DocumentType.JSON);

            var loader = new TableU1Loader();
            var result = loader.LoadFromJSON(filename);

            // save to database
            var repository = DataFactory.Instance.GetTableU1Repository(AppHelper.DatabaseConnectionString);
            repository.InsertTableData(result);
        }
    }
}
