using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Logic.Loaders;
using DataProcessingApp.Logic.Savers;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableCWorker
    {
        public static void LoadTableData()
        {
            var filename = FilesHelper.GenerateFilename(TableType.TableC, DocumentType.JSON);

            var loader = new TableCLoader();
            loader.LoadFromJSON(filename);
        }

        public static void ExportToExcel()
        {
            // 1. Load data from JSON file.
            var filename = FilesHelper.GenerateFilename(TableType.TableC, DocumentType.JSON);

            var loader = new TableCLoader();
            var result = loader.LoadFromJSON(filename);

            // 2. Save data to Excel document.
            var excelFilename = FilesHelper.GenerateFilename(TableType.TableC, DocumentType.Excel);

            var saver = new TableCSaver();
            saver.SaveToExcel(result, excelFilename);
        }
        
        public static void SaveToTextFileFile()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableC, DocumentType.JSON);

            var loader = new TableCLoader();
            var result = loader.LoadFromJSON(filename);

            // save
            var textFilename = FilesHelper.GenerateFilename(TableType.TableC, DocumentType.Text);

            var saver = new TableCSaver();
            saver.SaveToTextFile(textFilename, result);
        }
    }
}
