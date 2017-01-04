using System.Collections.Generic;

namespace DataProcessingApp.Core.DataObjects
{
    public class TableB
    {
        public List<TableBRow> Rows { get; set; }

        public TableB()
        {
            Rows = new List<TableBRow>();
        }
    }
}
