using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Data.Helpers;
using System.Collections.Generic;

namespace DataProcessingApp.Data.Repositories;

/// <summary>
/// Generic repository that reloads table rows into the SQL Server
/// destination table mapped from <see cref="TableType"/>: existing rows are
/// cleared first, so reruns never duplicate data.
/// </summary>
public class TableRepository<TRow> : BaseRepository
{
    public TableRepository(string connectionString, TableType tableType) : base(connectionString)
    {
        DestinationTableName = SqlTables.DestinationTable(tableType);
    }

    public string DestinationTableName { get; }

    /// <summary>
    /// Clears the destination table and bulk-inserts the rows.
    /// Returns the number of inserted rows.
    /// </summary>
    public int InsertTableData(IEnumerable<TRow> rows)
    {
        // create DataTable with data
        var dataTable = DataTableHelper.CreateDataTable(rows);

        ReloadTableData(dataTable, DestinationTableName);

        return dataTable.Rows.Count;
    }
}