using System.IO;
using System.Text;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Logic.Loaders;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableU1Worker
    {
        public static void LoadTableData()
        {
            var filename = "i:\\Exadel\\WK\\FinEstCOM Replacement\\tabula-TableU1-1990-processed.json";

            var loader = new TableU1Loader();
            var result = loader.LoadFromJSON(filename);

            SaveTableDataToFile(result);
        }

        private static void SaveTableDataToFile(TableU1 result)
        {
            var filename = "i:\\Exadel\\WK\\FinEstCOM Replacement\\tabula-TableU1-1990-processed-2.txt";

            var file = new StreamWriter(filename, false, Encoding.UTF8);
            foreach (var row in result.Rows)
            {
                file.WriteLine("{0} {1} {2} {3}", row.MortalityTable, row.Age, row.AdjustedPayoutRate, row.RemainderFactor);
            }
            file.Close();
        }
    }
}
