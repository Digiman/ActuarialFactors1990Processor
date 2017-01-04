using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Data.Helpers;

namespace DataProcessingApp.Data.Repositories
{
    public class TableDRepository : BaseRepository
    {
        public TableDRepository(string connectionString) : base(connectionString)
        {
        }

        public void InsertTableData(TableD table)
        {
            // create DataTable with data
            var dataTable = DataTableHelper.CreateDataTable(table);

            var destinationTableName = "[dbo].[tblD]";

            BulkInsertTableData(dataTable, destinationTableName);
        }
    }
}
