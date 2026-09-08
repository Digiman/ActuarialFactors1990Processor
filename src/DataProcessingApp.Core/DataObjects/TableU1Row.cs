namespace DataProcessingApp.Core.DataObjects;

public class TableU1Row
{
    [DbColumn("MortalityTable")]
    public int MortalityTable { get; set; }

    [DbColumn("Age")]
    public int Age { get; set; }

    [DbColumn("AdjustedPayoutRate")]
    [Round(1)]
    public double AdjustedPayoutRate { get; set; }

    [DbColumn("remainderFactor")]
    public double RemainderFactor { get; set; }
}