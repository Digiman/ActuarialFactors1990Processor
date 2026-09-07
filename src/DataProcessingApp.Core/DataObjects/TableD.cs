using System.Collections.Generic;

namespace DataProcessingApp.Core.DataObjects
{
    public class TableD
    {
        public List<TableDRow> Rows { get; set; }

        public TableD()
        {
            Rows = new List<TableDRow>();
        }
    }
}
