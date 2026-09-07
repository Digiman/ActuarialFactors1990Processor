namespace DataProcessingApp.Core.DataObjects
{
    public class TableSRow
    {
        [DbColumn("MortalityTable")]
        public int MortalityTable { get; set; }

        [DbColumn("InterestRate")]
        [Round(1)]
        public double InterestRate { get; set; }

        [DbColumn("Age")]
        public int Age { get; set; }

        [DbColumn("pvAnnuity")]
        public double PvAnnuity { get; set; }

        [DbColumn("pvLifeEstate")]
        public double PvLifeEstate { get; set; }

        [DbColumn("pvRemainderInterest")]
        public double PvReminderInterest { get; set; }
    }
}
