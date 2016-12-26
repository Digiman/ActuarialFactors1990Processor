using System;
using System.IO;
using System.Text;
using DataProcessingApp.Core;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Logic.Loaders;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableU2Worker
    {
        public static void LoadTableData()
        {
            var files = new[]
            {
                "tabula-TableU(2)-p1-1990",
                "tabula-TableU(2)-p2-1990",
                "tabula-TableU(2)-p3-1990",
                "tabula-TableU(2)-p4-1990",
                "tabula-TableU(2)-p5-1990"
            };

            foreach (var file in files)
            {
                LoadTablePartData(file);
            }
        }

        private static void LoadTablePartData(string baseFilename)
        {
            var filename = String.Format("{0}\\{1}-processed.json", Constants.BaseDatadir, baseFilename);

            var loader = new TableU2Loader();
            var result = loader.LoadFromJSON(filename);

            SaveTableDataToFile(result, baseFilename);
        }

        private static void SaveTableDataToFile(TableU2 result, string baseFilename)
        {
            var filename = String.Format("{0}\\{1}-processed-2.txt", Constants.BaseDatadir, baseFilename);

            var file = new StreamWriter(filename, false, Encoding.UTF8);
            foreach (var row in result.Rows)
            {
                file.WriteLine("{0} {1} {2} {3} {4}", row.MortalityTable, row.Age1, row.Age2, row.AdjustedPayoutRate, row.RemainderFactor);
            }
            file.Close();
        }
    }
}
