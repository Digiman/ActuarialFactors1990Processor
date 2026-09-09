namespace DataProcessingApp.Core.DataObjects;

public class TableU2Row
{
    [DbColumn("MortalityTable")]
    public int MortalityTable { get; set; }

    [DbColumn("Age1")]
    public int Age1 { get; set; }

    [DbColumn("Age2")]
    public int Age2 { get; set; }

    [DbColumn("AdjustedPayoutRate")]
    [Round(1)]
    public double AdjustedPayoutRate { get; set; }

    [DbColumn("remainderFactor")]
    public double RemainderFactor { get; set; }
}