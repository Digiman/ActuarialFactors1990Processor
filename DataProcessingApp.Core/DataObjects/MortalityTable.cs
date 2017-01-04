using System.Collections.Generic;

namespace DataProcessingApp.Core.DataObjects
{
    public class MortalityTable
    {
        public List<MortalityTableRow> Rows { get; set; }

        public MortalityTable()
        {
            Rows = new List<MortalityTableRow>();
        }
    }
}
