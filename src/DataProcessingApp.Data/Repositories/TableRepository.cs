using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Data.Helpers;
using System.Collections.Generic;

namespace DataProcessingApp.Data.Repositories;

/// <summary>
/// Generic repository that bulk-inserts table rows into the SQL Server
/// destination table mapped from <see cref="TableType"/>.
/// </summary>
public class TableRepository<TRow> : BaseRepository
{
    public TableRepository(string connectionString, TableType tableType) : base(connectionString)
    {
        DestinationTableName = SqlTables.DestinationTable(tableType);
    }

    public string DestinationTableName { get; }

    public void InsertTableData(IEnumerable<TRow> rows)
    {
        // create DataTable with data
        var dataTable = DataTableHelper.CreateDataTable(rows);

        BulkInsertTableData(dataTable, DestinationTableName);
    }
}