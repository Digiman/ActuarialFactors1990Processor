using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Data.Helpers;

namespace DataProcessingApp.Data.Repositories
{
    public class TableFRepository : BaseRepository
    {
        public TableFRepository(string connectionString) : base(connectionString)
        {
        }

        public void InsertTableData(TableF table)
        {
            // create DataTable with data
            var dataTable = DataTableHelper.CreateDataTable(table);

            var destinationTableName = "[dbo].[tblF]";

            BulkInsertTableData(dataTable, destinationTableName);
        }
    }
}
