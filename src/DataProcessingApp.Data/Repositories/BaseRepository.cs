using System;
using System.Data;
using Microsoft.Data.SqlClient;

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

                    // map columns by name so DataTable column order is irrelevant
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }

                    try
                    {
                        // send table with data to database
                        bulkCopy.WriteToServer(dataTable);
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}