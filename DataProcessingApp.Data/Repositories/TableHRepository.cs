using System;
using System.Data.SqlClient;
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
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                {
                    bulkCopy.BatchSize = 10000;
                    bulkCopy.DestinationTableName = "[dbo].[tblH]";
                    try
                    {
                        // create DataTable with data
                        var dataTable = DataTableHelper.CreateDataTable(table);

                        // send table with data to database
                        bulkCopy.WriteToServer(dataTable);
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        connection.Close();
                    }
                }

                transaction.Commit();
            }
        }
    }
}
