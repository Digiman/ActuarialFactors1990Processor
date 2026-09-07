using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Data.Helpers;

namespace DataProcessingApp.Data.Repositories
{
    public class TableSRepository : BaseRepository
    {
        public TableSRepository(string connectionString) : base(connectionString)
        {
        }

        public void InsertTableData(TableS table)
        {
            // create DataTable with data
            var dataTable = DataTableHelper.CreateDataTable(table);

            var destinationTableName = "[dbo].[tblS]";

            BulkInsertTableData(dataTable, destinationTableName);
        }
    }
}
