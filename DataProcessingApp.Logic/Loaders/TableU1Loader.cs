using System;
using System.Collections.Generic;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Logic.Loaders
{
    /// <summary>
    /// Class to load data for Table U(1).
    /// </summary>
    public class TableU1Loader : BaseLoader
    {
        public TableU1 LoadFromJSON(string filename)
        {
            var result = new TableU1();

            // load data from JSON file
            var data = LoadDataFromJsonFile<List<TableU1Row>>(filename);

            // do additional processing if needed
            ProcessData(ref data);

            result.Rows = data;

            // return result
            return result;
        }

        private void ProcessData(ref List<TableU1Row> data)
        {
            foreach (var row in data)
            {
                row.AdjustedPayoutRate = Math.Round(row.AdjustedPayoutRate, 1);
            }
        }
    }
}
