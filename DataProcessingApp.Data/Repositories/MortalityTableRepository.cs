using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Data.Helpers;

namespace DataProcessingApp.Data.Repositories
{
    public class MortalityTableRepository : BaseRepository
    {
        public MortalityTableRepository(string connectionString) : base(connectionString)
        {
        }

        public void InsertTableData(MortalityTable table)
        {
            // create DataTable with data
            var dataTable = DataTableHelper.CreateDataTable(table);

            var destinationTableName = "[dbo].[tblMortality]";

            BulkInsertTableData(dataTable, destinationTableName);
        }
    }
}
