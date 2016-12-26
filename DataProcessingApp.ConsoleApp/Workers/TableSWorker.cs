using System;
using System.IO;
using System.Text;
using DataProcessingApp.Core;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Logic.Loaders;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableHWorker
    {
        public static void LoadTableData()
        {
            var filename = String.Format("{0}\\tabula-TableH-1990-processed.json", Constants.BaseDatadir);

            var loader = new TableHLoader();
            var result = loader.LoadFromJSON(filename);

            SaveTableDataToFile(result);
        }

        private static void SaveTableDataToFile(TableH result)
        {
            var filename = String.Format("{0}\\tabula-TableH-1990-processed-2.txt", Constants.BaseDatadir);

            var file = new StreamWriter(filename, false, Encoding.UTF8);
            foreach (var row in result.Rows)
            {
                file.WriteLine("{0} {1} {2} {3} {4} {5}", row.MortalityTable, row.InterestRate, row.Age, row.DFactor, row.NFactor, row.MFactor);
            }
            file.Close();
        }
    }
}
