using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Data.Helpers;

namespace DataProcessingApp.Data.Repositories
{
    public class TableCRepository : BaseRepository
    {
        public TableCRepository(string connectionString) : base(connectionString)
        {
        }

        public void InsertTableData(TableC table)
        {
            // create DataTable with data
            var dataTable = DataTableHelper.CreateDataTable(table);

            var destinationTableName = "[dbo].[tblC]";

            BulkInsertTableData(dataTable, destinationTableName);
        }
    }
}
