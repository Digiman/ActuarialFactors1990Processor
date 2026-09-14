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
    /// Pass clearYear to delete only the rows of that MortalityTable year
    /// (the series-backed tables share one table across series).
    /// Returns the number of inserted rows.
    /// </summary>
    public int InsertTableData(IEnumerable<TRow> rows, int? clearYear = null)
    {
        // create DataTable with data
        var dataTable = DataTableHelper.CreateDataTable(rows);

        if (clearYear.HasValue)
        {
            ReloadTableData(dataTable, DestinationTableName, "MortalityTable", clearYear.Value);
        }
        else
        {
            ReloadTableData(dataTable, DestinationTableName);
        }

        return dataTable.Rows.Count;
    }
}