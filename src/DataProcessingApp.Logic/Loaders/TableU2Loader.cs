using System;
using System.Collections.Generic;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Logic.Loaders
{
    /// <summary>
    /// Class to load data for Table U(2).
    /// </summary>
    public class TableU2Loader : BaseLoader
    {
        public TableU2 LoadFromJSON(string filename)
        {
            var result = new TableU2();

            // load data from JSON file
            var data = LoadDataFromJsonFile<List<TableU2Row>>(filename);

            // do additional processing if needed
            ProcessData(ref data);

            result.Rows = data;

            // return result
            return result;
        }

        private void ProcessData(ref List<TableU2Row> data)
        {
            foreach (var row in data)
            {
                row.AdjustedPayoutRate = Math.Round(row.AdjustedPayoutRate, 1);
            }
        }
    }
}
