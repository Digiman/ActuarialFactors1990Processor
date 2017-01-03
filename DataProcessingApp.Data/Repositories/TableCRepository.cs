﻿using System;
using System.Data.SqlClient;
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
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                {
                    bulkCopy.BatchSize = 10000;
                    bulkCopy.DestinationTableName = "[dbo].[tblC]";
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