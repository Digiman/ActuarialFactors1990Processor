using System.Data;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Data.Helpers
{
    public static class DataTableHelper
    {
        #region Table headers.

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

        private static readonly string[] MortalityTableHeader =
        {
            "Year", "Age", "lx"
        };

        private static readonly string[] TableBHeader =
        {
            "Years", "Rate", "pvAnnuity", "pvIncomeInterest", "pvRemainderInterest"
        };

        private static readonly string[] TableDHeader =
        {
            "Year", "PayoutRate", "remainderInteres"
        };

        private static readonly string[] TableFHeader =
        {
            "InterestRate", "Frequency", "Months", "adjustmentFactor"
        };

        private static readonly string[] TableJHeader =
        {
            "InterestRate", "Frequency", "adjustmentFactor"
        };

        private static readonly string[] TableKHeader =
        {
            "InterestRate", "Frequency", "adjustmentFactor"
        };

        #endregion

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

        public static DataTable CreateDataTable(MortalityTable table)
        {
            var dataTable = new DataTable();

            // add columns
            foreach (var header in MortalityTableHeader)
            {
                dataTable.Columns.Add(header);
            }

            // add data rows
            foreach (var row in table.Rows)
            {
                dataTable.Rows.Add(row.Year, row.Age, row.Lx);
            }

            return dataTable;
        }

        public static DataTable CreateDataTable(TableB table)
        {
            var dataTable = new DataTable();

            // add columns
            foreach (var header in TableBHeader)
            {
                dataTable.Columns.Add(header);
            }

            // add data rows
            foreach (var row in table.Rows)
            {
                dataTable.Rows.Add(row.Years, row.Rate, row.PvAnnuity, row.PvIncomeInterest, row.PvRemainderInterest);
            }

            return dataTable;
        }

        public static DataTable CreateDataTable(TableD table)
        {
            var dataTable = new DataTable();

            // add columns
            foreach (var header in TableDHeader)
            {
                dataTable.Columns.Add(header);
            }

            // add data rows
            foreach (var row in table.Rows)
            {
                dataTable.Rows.Add(row.Years, row.PayoutRate, row.RemainderInterest);
            }

            return dataTable;
        }

        public static DataTable CreateDataTable(TableF table)
        {
            var dataTable = new DataTable();

            // add columns
            foreach (var header in TableFHeader)
            {
                dataTable.Columns.Add(header);
            }

            // add data rows
            foreach (var row in table.Rows)
            {
                dataTable.Rows.Add(row.InterestRate, row.Frequency, row.Months, row.AdjustmentFactor);
            }

            return dataTable;
        }

        public static DataTable CreateDataTable(TableJ table)
        {
            var dataTable = new DataTable();

            // add columns
            foreach (var header in TableJHeader)
            {
                dataTable.Columns.Add(header);
            }

            // add data rows
            foreach (var row in table.Rows)
            {
                dataTable.Rows.Add(row.InterestRate, row.Frequency, row.AdjustmentFactor);
            }

            return dataTable;
        }

        public static DataTable CreateDataTable(TableK table)
        {
            var dataTable = new DataTable();

            // add columns
            foreach (var header in TableKHeader)
            {
                dataTable.Columns.Add(header);
            }

            // add data rows
            foreach (var row in table.Rows)
            {
                dataTable.Rows.Add(row.InterestRate, row.Frequency, row.AdjustmentFactor);
            }

            return dataTable;
        }
    }
}
