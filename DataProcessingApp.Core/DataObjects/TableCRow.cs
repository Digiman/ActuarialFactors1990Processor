namespace DataProcessingApp.Core.DataObjects
{
    public class TableCRow
    {
        public int MortalityTable { get; set; }
        public double Rate { get; set; }
        public int Age { get; set; }
        public double RemainderFactor { get; set; }
        public double RFactor { get; set; }
        public double DFactor { get; set; }

        /*public TableCRow()
        {
            MortalityTable = Constants.DefaultMortalityYear;
        }*/
    }
}
