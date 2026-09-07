using System.Collections.Generic;

namespace DataProcessingApp.Core.DataObjects
{
    public class TableF
    {
        public List<TableFRow> Rows { get; set; }

        public TableF()
        {
            Rows = new List<TableFRow>();
        }
    }
}
