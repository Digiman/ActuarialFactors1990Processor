using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Data.Helpers;

namespace DataProcessingApp.Data.Repositories
{
    public class TableJRepository : BaseRepository
    {
        public TableJRepository(string connectionString) : base(connectionString)
        {
        }

        public void InsertTableData(TableJ table)
        {
            // create DataTable with data
            var dataTable = DataTableHelper.CreateDataTable(table);

            var destinationTableName = "[dbo].[tblJ]";

            BulkInsertTableData(dataTable, destinationTableName);
        }
    }
}
