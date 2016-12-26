using System.Collections.Generic;

namespace DataProcessingApp.Core.DataObjects
{
    public class TableC
    {
        public List<TableCRow> Rows { get; set; }

        public TableC()
        {
            Rows = new List<TableCRow>();
        }
    }
}
