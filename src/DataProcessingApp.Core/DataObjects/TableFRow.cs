namespace DataProcessingApp.Core.DataObjects;

public class TableFRow
{
    [DbColumn("InterestRate")]
    public double InterestRate { get; set; }

    [DbColumn("Frequency")]
    public string Frequency { get; set; }

    [DbColumn("Months")]
    public int Months { get; set; }

    [DbColumn("adjustmentFactor")]
    public double AdjustmentFactor { get; set; }
}