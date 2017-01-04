using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Data;
using DataProcessingApp.Logic.Loaders;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class MortalityTableWorker
    {
        public static void LoadTableData()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.MortalityTable, DocumentType.XML);

            var loader = new MortalityTableLoader();
            loader.LoadFromXML(filename);
        }

        public static void SaveToDatabase()
        {
            // load data
            var filename = FilesHelper.GenerateFilename(TableType.MortalityTable, DocumentType.XML);

            var loader = new MortalityTableLoader();
            var result = loader.LoadFromXML(filename);

            // save to database
            var repository = DataFactory.Instance.GetMortalityTableRepository(AppHelper.DatabaseConnectionString);
            repository.InsertTableData(result);
        }
    }
}
