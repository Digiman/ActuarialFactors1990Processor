using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DataProcessingApp.Core;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Logic.Loaders;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableR2Worker
    {
        static readonly string[] Files = new[]
            {
                "tabula-TableR(2)-p1-1990",
                "tabula-TableR(2)-p2-1990",
                "tabula-TableR(2)-p3-1990",
                "tabula-TableR(2)-p4-1990",
                "tabula-TableR(2)-p5-1990"
            };

        public static void LoadTableData()
        {
            foreach (var file in Files)
            {
                var data = LoadTablePartData(file);

                SaveTableDataToFile(data, file);
            }
        }

        public static void CombineTableParts()
        {
            var tableParts = new List<TableR2>();

            foreach (var file in Files)
            {
                tableParts.Add(LoadTablePartData(file));
            }

            var combinedTable = CreateOneTable(tableParts);

            SaveTableDataToJSON(combinedTable, "tabula-TableR(2)-full-1990");

            SaveTableDataToFile(combinedTable, "tabula-TableR(2)-full-1990");
        }
        
        private static TableR2 CreateOneTable(List<TableR2> tableParts)
        {
            var table = new TableR2();

            foreach (var tablePart in tableParts)
            {
                table.Rows.AddRange(tablePart.Rows);
            }

            return table;
        }

        private static TableR2 LoadTablePartData(string baseFilename)
        {
            var filename = String.Format("{0}\\{1}-processed.json", Constants.BaseDatadir, baseFilename);

            var loader = new TableR2Loader();
            var result = loader.LoadFromJSON(filename);

            return result;
        }

        private static void SaveTableDataToFile(TableR2 result, string baseFilename)
        {
            var filename = String.Format("{0}\\{1}-processed-2.txt", Constants.BaseDatadir, baseFilename);

            var file = new StreamWriter(filename, false, Encoding.UTF8);
            foreach (var row in result.Rows)
            {
                file.WriteLine("{0} {1} {2} {3} {4}", row.MortalityTable, row.Age1, row.Age2, row.AdjustedPayoutRate, row.RemainderFactor);
            }
            file.Close();
        }

        private static void SaveTableDataToJSON(TableR2 table, string baseFilename)
        {
            var filename = String.Format("{0}\\{1}-processed.json", Constants.BaseDatadir, baseFilename);

            var jsonData = SerializerHelper.Serialize(table.Rows, SerializeFormat.JSON);

            File.WriteAllText(filename, jsonData);
        }
    }
}
