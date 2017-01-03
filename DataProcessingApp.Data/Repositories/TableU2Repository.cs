using System;
using System.Data.SqlClient;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Data.Helpers;

namespace DataProcessingApp.Data.Repositories
{
    public class TableU2Repository : BaseRepository
    {
        public TableU2Repository(string connectionString) : base(connectionString)
        {
        }
        
        public void InsertTableData(TableU2 table)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                {
                    bulkCopy.BatchSize = 10000;
                    bulkCopy.DestinationTableName = "[dbo].[tblU2]";
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
