using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Data.Helpers;

namespace DataProcessingApp.Data.Repositories
{
    public class TableBRepository : BaseRepository
    {
        public TableBRepository(string connectionString) : base(connectionString)
        {
        }

        public void InsertTableData(TableB table)
        {
            // create DataTable with data
            var dataTable = DataTableHelper.CreateDataTable(table);

            var destinationTableName = "[dbo].[tblB]";

            BulkInsertTableData(dataTable, destinationTableName);
        }
    }
}
