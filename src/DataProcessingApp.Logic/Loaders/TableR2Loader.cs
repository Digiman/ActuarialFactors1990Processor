using System;
using System.Collections.Generic;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Logic.Loaders
{
    /// <summary>
    /// Class to load data for Table R(2).
    /// </summary>
    public class TableR2Loader : BaseLoader
    {
        public TableR2 LoadFromJSON(string filename)
        {
            var result = new TableR2();

            // load data from JSON file
            var data = LoadDataFromJsonFile<List<TableR2Row>>(filename);

            // do additional processing if needed
            ProcessData(ref data);

            result.Rows = data;

            // return result
            return result;
        }

        private void ProcessData(ref List<TableR2Row> data)
        {
            foreach (var row in data)
            {
                row.AdjustedPayoutRate = Math.Round(row.AdjustedPayoutRate, 1);
            }
        }
    }
}
