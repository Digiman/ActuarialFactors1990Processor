using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Data.Helpers;

namespace DataProcessingApp.Data.Repositories
{
    public class TableR2Repository : BaseRepository
    {
        public TableR2Repository(string connectionString) : base(connectionString)
        {
        }
        
        public void InsertTableData(TableR2 table)
        {
            // create DataTable with data
            var dataTable = DataTableHelper.CreateDataTable(table);

            var destinationTableName = "[dbo].[tblR2]";

            BulkInsertTableData(dataTable, destinationTableName, 100000);
        }
    }
}
