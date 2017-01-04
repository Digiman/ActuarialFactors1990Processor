using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Data.Helpers;

namespace DataProcessingApp.Data.Repositories
{
    public class TableU1Repository : BaseRepository
    {
        public TableU1Repository(string connectionString) : base(connectionString)
        {
        }
        
        public void InsertTableData(TableU1 table)
        {
            // create DataTable with data
            var dataTable = DataTableHelper.CreateDataTable(table);

            var destinationTableName = "[dbo].[tblU1]";

            BulkInsertTableData(dataTable, destinationTableName);
        }
    }
}
