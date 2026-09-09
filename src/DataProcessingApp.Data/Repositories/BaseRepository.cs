using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DataProcessingApp.Data.Repositories;

public class BaseRepository
{
    private string ConnectionString { get; set; }

    protected BaseRepository(string connectionString)
    {
        ConnectionString = connectionString;
    }

    /// <summary>
    /// Clears the destination table and bulk-inserts the new rows in one
    /// transaction, so reruns are idempotent and a failure leaves the table
    /// untouched.
    /// </summary>
    protected void ReloadTableData(DataTable dataTable, string destinationTableName, int batchSize = 10000)
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
                    // clear existing rows first; rolled back together with the
                    // insert if the bulk copy fails
                    ClearTableData(transaction, destinationTableName);

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

    private static void ClearTableData(SqlTransaction transaction, string destinationTableName)
    {
        using (var command = new SqlCommand($"TRUNCATE TABLE {destinationTableName}", transaction.Connection, transaction))
        {
            command.ExecuteNonQuery();
        }
    }
}