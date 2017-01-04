using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Data.Helpers;

namespace DataProcessingApp.Data.Repositories
{
    public class TableKRepository : BaseRepository
    {
        public TableKRepository(string connectionString) : base(connectionString)
        {
        }

        public void InsertTableData(TableK table)
        {
            // create DataTable with data
            var dataTable = DataTableHelper.CreateDataTable(table);

            var destinationTableName = "[dbo].[tblK]";

            BulkInsertTableData(dataTable, destinationTableName);
        }
    }
}
