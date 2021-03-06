using System;
using System.Data;
using System.Data.SqlClient;

namespace DataProcessingApp.Data.Repositories
{
    public class BaseRepository
    {
        private string ConnectionString { get; set; }

        protected BaseRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected void BulkInsertTableData(DataTable dataTable, string destinationTableName, int batchSize = 10000)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                {
                    bulkCopy.BatchSize = batchSize;
                    bulkCopy.DestinationTableName = destinationTableName;
                    try
                    {
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