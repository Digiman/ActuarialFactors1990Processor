namespace DataProcessingApp.Core.DataObjects
{
    public class TableJRow
    {
        [DbColumn("InterestRate")]
        public double InterestRate { get; set; }

        [DbColumn("Frequency")]
        public string Frequency { get; set; }

        [DbColumn("adjustmentFactor")]
        public double AdjustmentFactor { get; set; }
    }
}
