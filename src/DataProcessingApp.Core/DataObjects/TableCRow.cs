namespace DataProcessingApp.Core.DataObjects;

public class TableCRow
{
    [DbColumn("MortalityTable")]
    public int MortalityTable { get; set; }

    [DbColumn("Rate")]
    [Round(1)]
    public double Rate { get; set; }

    [DbColumn("Age")]
    public int Age { get; set; }

    [DbColumn("remainderFactor")]
    public double RemainderFactor { get; set; }

    [DbColumn("rFactor")]
    public double RFactor { get; set; }

    [DbColumn("dFactor")]
    public double DFactor { get; set; }
}