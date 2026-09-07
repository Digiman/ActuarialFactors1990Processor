namespace DataProcessingApp.Core.DataObjects
{
    public class MortalityTableRow
    {
        [DbColumn("Year")]
        public int Year { get; set; }

        [DbColumn("Age")]
        public int Age { get; set; }

        [DbColumn("lx")]
        public int Lx { get; set; }
    }
}
