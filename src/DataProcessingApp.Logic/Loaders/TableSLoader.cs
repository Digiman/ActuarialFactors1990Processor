using System;
using System.Collections.Generic;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Logic.Loaders
{
    /// <summary>
    /// Class to load data for Table S.
    /// </summary>
    public class TableSLoader : BaseLoader
    {
        public TableS LoadFromJSON(string filename)
        {
            var result = new TableS();

            // load data from JSON file
            var data = LoadDataFromJsonFile<List<TableSRow>>(filename);

            // do additional processing if needed
            ProcessData(ref data);

            result.Rows = data;

            // return result
            return result;
        }

        private void ProcessData(ref List<TableSRow> data)
        {
            foreach (var row in data)
            {
                row.InterestRate = Math.Round(row.InterestRate, 1);
            }
        }
    }
}
