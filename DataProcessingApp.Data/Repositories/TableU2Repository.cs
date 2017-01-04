using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Data.Helpers;

namespace DataProcessingApp.Data.Repositories
{
    public class TableU2Repository : BaseRepository
    {
        public TableU2Repository(string connectionString) : base(connectionString)
        {
        }
        
        public void InsertTableData(TableU2 table)
        {
            // create DataTable with data
            var dataTable = DataTableHelper.CreateDataTable(table);

            var destinationTableName = "[dbo].[tblU2]";

            BulkInsertTableData(dataTable, destinationTableName);
        }
    }
}
