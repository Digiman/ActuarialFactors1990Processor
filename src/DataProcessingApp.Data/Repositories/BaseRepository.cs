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
        ReloadTableData(dataTable, destinationTableName, null, 0, batchSize);
    }

    /// <summary>
    /// Same as <see cref="ReloadTableData(DataTable, string, int)"/>, but the
    /// clear step deletes only the rows whose MortalityTable column equals
    /// clearYear: the series-backed tables share one destination table across
    /// all series, so truncating per series would wipe the other series.
    /// </summary>
    protected void ReloadTableData(DataTable dataTable, string destinationTableName, string clearYearColumn, int clearYear, int batchSize = 10000)
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
                    if (clearYearColumn == null)
                    {
                        ClearTableData(transaction, destinationTableName);
                    }
                    else
                    {
                        ClearTableData(transaction, destinationTableName, clearYearColumn, clearYear);
                    }

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

    private static void ClearTableData(SqlTransaction transaction, string destinationTableName, string clearYearColumn, int clearYear)
    {
        using (var command = new SqlCommand(
            $"DELETE FROM {destinationTableName} WHERE [{clearYearColumn}] = @year",
            transaction.Connection, transaction))
        {
            command.Parameters.AddWithValue("@year", clearYear);
            command.ExecuteNonQuery();
        }
    }
}