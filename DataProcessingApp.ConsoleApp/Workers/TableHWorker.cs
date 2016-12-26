﻿using System.IO;
using System.Text;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Logic.Loaders;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableSWorker
    {
        public static void LoadTableData()
        {
            var filename = "i:\\Exadel\\WK\\FinEstCOM Replacement\\tabula-TableS-1990-processed.json";

            var loader = new TableSLoader();
            var result = loader.LoadFromJSON(filename);

            SaveTableDataToFile(result);
        }

        private static void SaveTableDataToFile(TableS result)
        {
            var filename = "i:\\Exadel\\WK\\FinEstCOM Replacement\\tabula-TableS-1990-processed-2.txt";

            var file = new StreamWriter(filename, false, Encoding.UTF8);
            foreach (var row in result.Rows)
            {
                file.WriteLine("{0} {1} {2} {3} {4} {5}", row.MortalityTable, row.InterestRate, row.Age, row.PvAnnuity, row.PvLifeEstate, row.PvReminderInterest);
            }
            file.Close();
        }
    }
}
