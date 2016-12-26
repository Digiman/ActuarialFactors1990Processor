using System.IO;
using System.Text;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Logic.Loaders;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableHWorker
    {
        public static void LoadTableData()
        {
            var filename = "i:\\Exadel\\WK\\FinEstCOM Replacement\\tabula-TableH-1990-processed.json";

            var loader = new TableHLoader();
            var result = loader.LoadFromJSON(filename);

            SaveTableHDataToFile(result);
        }

        private static void SaveTableHDataToFile(TableH result)
        {
            var filename = "i:\\Exadel\\WK\\FinEstCOM Replacement\\tabula-TableH-1990-processed-2.txt";

            var file = new StreamWriter(filename, false, Encoding.UTF8);
            foreach (var row in result.Rows)
            {
                file.WriteLine("{0} {1} {2} {3} {4}", row.InterestRate, row.Age, row.DFactor, row.NFactor, row.MFactor);
            }
            file.Close();
        }
    }
}
