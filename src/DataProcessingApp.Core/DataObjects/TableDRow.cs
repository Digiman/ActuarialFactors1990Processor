namespace DataProcessingApp.Core.DataObjects
{
    public class TableDRow
    {
        [DbColumn("Years")]
        public int Years { get; set; }

        [DbColumn("PayoutRate")]
        public double PayoutRate { get; set; }

        [DbColumn("remainderInterest")]
        public double RemainderInterest { get; set; }
    }
}
