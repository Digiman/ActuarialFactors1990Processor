using System;
using System.Collections.Generic;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Logic.Loaders
{
    /// <summary>
    /// Class to load data for Table H.
    /// </summary>
    public class TableHLoader : BaseLoader
    {
        public TableH LoadFromJSON(string filename)
        {
            var result = new TableH();

            // load data from JSON file
            var data = LoadDataFromJsonFile<List<TableHRow>>(filename);

            // do additional processing if needed
            ProcessData(ref data);

            result.Rows = data;

            // return result
            return result;
        }

        private void ProcessData(ref List<TableHRow> data)
        {
            foreach (var row in data)
            {
                row.InterestRate = Math.Round(row.InterestRate, 1);
            }
        }
    }
}
