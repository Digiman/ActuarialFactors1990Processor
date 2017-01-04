using System.Collections.Generic;

namespace DataProcessingApp.Core.DataObjects
{
    public class TableJ
    {
        public List<TableJRow> Rows { get; set; }

        public TableJ()
        {
            Rows = new List<TableJRow>();
        }
    }
}
