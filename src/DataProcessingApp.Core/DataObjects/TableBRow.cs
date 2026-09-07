namespace DataProcessingApp.Core.DataObjects
{
    public class TableBRow
    {
        [DbColumn("Years")]
        public double Years { get; set; }

        [DbColumn("Rate")]
        public double Rate { get; set; }

        [DbColumn("pvAnnuity")]
        public double PvAnnuity { get; set; }

        [DbColumn("pvIncomeInterest")]
        public double PvIncomeInterest { get; set; }

        [DbColumn("pvRemainderInterest")]
        public double PvRemainderInterest { get; set; }
    }
}
