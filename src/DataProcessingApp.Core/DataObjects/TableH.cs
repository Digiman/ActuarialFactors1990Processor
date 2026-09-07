using System.Collections.Generic;

namespace DataProcessingApp.Core.DataObjects
{
    public class TableH
    {
        public List<TableHRow> Rows { get; set; }

        public TableH()
        {
            Rows = new List<TableHRow>();
        }
    }
}
