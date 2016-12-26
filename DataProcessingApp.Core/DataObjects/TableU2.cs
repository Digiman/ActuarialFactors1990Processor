using System.Collections.Generic;

namespace DataProcessingApp.Core.DataObjects
{
    public class TableU2
    {
        public List<TableU2Row> Rows { get; set; }

        public TableU2()
        {
            Rows = new List<TableU2Row>();
        }
    }
}
