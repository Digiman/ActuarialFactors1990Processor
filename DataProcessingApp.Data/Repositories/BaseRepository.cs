using System;
using System.Data;
using System.Data.SqlClient;

namespace DataProcessingApp.Data.Repositories
{
    public class BaseRepository
    {
        protected string ConnectionString { get; set; }

        public BaseRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected void BulkInsertTableData(DataTable dataTable, string destinationTableName)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                {
                    bulkCopy.BatchSize = 10000;
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