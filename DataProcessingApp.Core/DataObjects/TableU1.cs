using System.Collections.Generic;

namespace DataProcessingApp.Core.DataObjects
{
    public class TableU1
    {
        public List<TableU1Row> Rows { get; set; }

        public TableU1()
        {
            Rows = new List<TableU1Row>();
        }
    }
}
