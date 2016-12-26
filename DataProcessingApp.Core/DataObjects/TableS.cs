using System.Collections.Generic;

namespace DataProcessingApp.Core.DataObjects
{
    public class TableS
    {
        public List<TableSRow> Rows { get; set; }

        public TableS()
        {
            Rows = new List<TableSRow>();
        }
    }
}
