using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Data.Helpers;

namespace DataProcessingApp.Data.Repositories
{
    public class TableHRepository : BaseRepository
    {
        public TableHRepository(string connectionString) : base(connectionString)
        {
        }

        public void InsertTableData(TableH table)
        {
            // create DataTable with data
            var dataTable = DataTableHelper.CreateDataTable(table);

            var destinationTableName = "[dbo].[tblH]";
            
            BulkInsertTableData(dataTable, destinationTableName);
        }
    }
}
