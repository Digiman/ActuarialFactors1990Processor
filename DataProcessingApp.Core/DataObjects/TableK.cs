using System.Collections.Generic;

namespace DataProcessingApp.Core.DataObjects
{
    public class TableK
    {
        public List<TableKRow> Rows { get; set; }

        public TableK()
        {
            Rows = new List<TableKRow>();
        }
    }
}
