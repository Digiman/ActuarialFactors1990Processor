using System.Data;
using System.Linq;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Data.Helpers;
using Xunit;

namespace DataProcessingApp.Tests
{
    public class DataTableHelperTests
    {
        [Fact]
        public void CreateDataTable_UsesDbColumnNamesInPropertyOrder()
        {
            var rows = new[] { new TableKRow { InterestRate = 0.2, Frequency = "Annual", AdjustmentFactor = 1.0 } };

            var dataTable = DataTableHelper.CreateDataTable(rows);

            Assert.Equal(new[] { "InterestRate", "Frequency", "adjustmentFactor" },
                dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
        }

        [Fact]
        public void CreateDataTable_MapsTableSReminderInterestToDbName()
        {
            // TableSRow.PvReminderInterest maps to the pvRemainderInterest DB column
            var rows = new[] { new TableSRow { MortalityTable = 1990, InterestRate = 2.2, Age = 0,
                PvAnnuity = 35.7962, PvLifeEstate = 0.78752, PvReminderInterest = 0.21248 } };

            var dataTable = DataTableHelper.CreateDataTable(rows);

            Assert.Equal(new[] { "MortalityTable", "InterestRate", "Age", "pvAnnuity", "pvLifeEstate", "pvRemainderInterest" },
                dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            Assert.Equal(1990, dataTable.Rows[0]["MortalityTable"]);
            Assert.Equal(0.21248, dataTable.Rows[0]["pvRemainderInterest"]);
        }

        [Fact]
        public void CreateDataTable_TypedColumns()
        {
            var rows = new[] { new MortalityTableRow { Year = 1990, Age = 0, Lx = 100000 } };

            var dataTable = DataTableHelper.CreateDataTable(rows);

            Assert.Equal(typeof(int), dataTable.Columns["Year"].DataType);
            Assert.Equal(typeof(int), dataTable.Columns["lx"].DataType);
        }
    }
}
