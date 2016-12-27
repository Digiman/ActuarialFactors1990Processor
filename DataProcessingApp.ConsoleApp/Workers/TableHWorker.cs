using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Logic.Loaders;
using DataProcessingApp.Logic.Savers;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableHWorker
    {
        public static void LoadTableData()
        {
            var filename = FilesHelper.GenerateFilename(TableType.TableH, DocumentType.JSON);

            var loader = new TableHLoader();
            loader.LoadFromJSON(filename);
        }

        public static void ExportToExcel()
        {
            // 1. Load data from JSON file.
            var filename = FilesHelper.GenerateFilename(TableType.TableH, DocumentType.JSON);

            var loader = new TableHLoader();
            var result = loader.LoadFromJSON(filename);

            // 2. Save data to Excel document.
            var excelFilename = FilesHelper.GenerateFilename(TableType.TableH, DocumentType.Excel);

            var saver = new TableHSaver();
            saver.SaveToExcel(result, excelFilename);
        }

        public static void SaveToTextFileFile()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.TableH, DocumentType.JSON);

            var loader = new TableHLoader();
            var result = loader.LoadFromJSON(filename);

            // save
            var textFilename = FilesHelper.GenerateFilename(TableType.TableH, DocumentType.Text);

            var saver = new TableHSaver();
            saver.SaveToTextFile(textFilename, result);
        }
    }
}
