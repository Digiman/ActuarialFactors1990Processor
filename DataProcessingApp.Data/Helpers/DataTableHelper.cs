using System.Data;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Data.Helpers
{
    public static class DataTableHelper
    {
        private static readonly string[] TableHHeader =
        {
            "MortalityTable", "Age", "InterestRate", "dFactor", "nFactor", "mFactor"
        };

        private static readonly string[] TableCHeader =
        {
            "MortalityTable", "Age", "Rate", "remainderFactor", "rFactor", "dFactor"
        };

        private static readonly string[] TableSHeader =
        {
            "MortalityTable", "Age", "InterestRate", "pvAnnuity", "pvLifeEstate", "pvReminderInterest"
        };

        private static readonly string[] TableU1Header =
        {
            "MortalityTable", "Age", "AdjustedPayoutRate", "remainderFactor"
        };

        private static readonly string[] TableU2Header =
        {
            "MortalityTable", "Age1", "Age2", "AdjustedPayoutRate", "remainderFactor"
        };

        private static readonly string[] TableR2Header =
        {
            "MortalityTable", "Age1", "Age2", "AdjustedPayoutRate", "remainderFactor"
        };

        public static DataTable CreateDataTable(TableH table)
        {
            var dataTable = new DataTable();

            // add columns
            foreach (var header in TableHHeader)
            {
                dataTable.Columns.Add(header);
            }

            // add data rows
            foreach (var row in table.Rows)
            {
                dataTable.Rows.Add(row.MortalityTable, row.Age, row.InterestRate, row.DFactor,
                    row.NFactor, row.MFactor);
            }

            return dataTable;
        }

        public static DataTable CreateDataTable(TableC table)
        {
            var dataTable = new DataTable();

            // add columns
            foreach (var header in TableCHeader)
            {
                dataTable.Columns.Add(header);
            }

            // add data rows
            foreach (var row in table.Rows)
            {
                dataTable.Rows.Add(row.MortalityTable, row.Age, row.Rate, row.RemainderFactor, row.RFactor, row.DFactor);
            }

            return dataTable;
        }

        public static DataTable CreateDataTable(TableS table)
        {
            var dataTable = new DataTable();

            // add columns
            foreach (var header in TableSHeader)
            {
                dataTable.Columns.Add(header);
            }

            // add data rows
            foreach (var row in table.Rows)
            {
                dataTable.Rows.Add(row.MortalityTable, row.Age, row.InterestRate, row.PvAnnuity, row.PvLifeEstate,
                    row.PvReminderInterest);
            }

            return dataTable;
        }

        public static DataTable CreateDataTable(TableU1 table)
        {
            var dataTable = new DataTable();

            // add columns
            foreach (var header in TableU1Header)
            {
                dataTable.Columns.Add(header);
            }

            // add data rows
            foreach (var row in table.Rows)
            {
                dataTable.Rows.Add(row.MortalityTable, row.Age, row.AdjustedPayoutRate, row.RemainderFactor);
            }

            return dataTable;
        }

        public static DataTable CreateDataTable(TableU2 table)
        {
            var dataTable = new DataTable();

            // add columns
            foreach (var header in TableU2Header)
            {
                dataTable.Columns.Add(header);
            }

            // add data rows
            foreach (var row in table.Rows)
            {
                dataTable.Rows.Add(row.MortalityTable, row.Age1, row.Age2, row.AdjustedPayoutRate, row.RemainderFactor);
            }

            return dataTable;
        }

        public static DataTable CreateDataTable(TableR2 table)
        {
            var dataTable = new DataTable();

            // add columns
            foreach (var header in TableR2Header)
            {
                dataTable.Columns.Add(header);
            }

            // add data rows
            foreach (var row in table.Rows)
            {
                dataTable.Rows.Add(row.MortalityTable, row.Age1, row.Age2, row.AdjustedPayoutRate, row.RemainderFactor);
            }

            return dataTable;
        }
    }
}
