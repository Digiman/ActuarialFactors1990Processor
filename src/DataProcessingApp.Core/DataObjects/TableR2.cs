using System.Collections.Generic;

namespace DataProcessingApp.Core.DataObjects
{
    public class TableR2
    {
        public List<TableR2Row> Rows { get; set; }

        public TableR2()
        {
            Rows = new List<TableR2Row>();
        }
    }
}
