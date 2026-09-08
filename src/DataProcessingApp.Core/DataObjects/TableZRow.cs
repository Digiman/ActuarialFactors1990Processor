namespace DataProcessingApp.Core.DataObjects;

public class TableZRow
{
    [DbColumn("MortalityTable")]
    public int MortalityTable { get; set; }

    [DbColumn("InterestRate")]
    [Round(1)]
    public double InterestRate { get; set; }

    [DbColumn("Age")]
    public int Age { get; set; }

    [DbColumn("dFactor")]
    public double DFactor { get; set; }

    [DbColumn("nFactor")]
    public double NFactor { get; set; }

    [DbColumn("mFactor")]
    public double MFactor { get; set; }
}
