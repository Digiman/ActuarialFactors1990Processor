using System;
using System.Collections.Generic;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Logic.Loaders
{
    /// <summary>
    /// Class to load data for Table C.
    /// </summary>
    public class TableCLoader : BaseLoader
    {
        public TableC LoadFromJSON(string filename)
        {
            var result = new TableC();

            // load data from JSON file
            var data = LoadDataFromJsonFile<List<TableCRow>>(filename);

            // do additional processing if needed
            ProcessData(ref data);

            result.Rows = data;

            // return result
            return result;
        }

        private void ProcessData(ref List<TableCRow> data)
        {
            foreach (var row in data)
            {
                row.Rate = Math.Round(row.Rate, 1);
            }
        }
    }
}
