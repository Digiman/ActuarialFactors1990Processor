using System.Collections.Generic;

namespace DataProcessingApp.Core.DataObjects;

/// <summary>
/// Generic container for actuarial table rows.
/// </summary>
public class TableData<TRow>
{
    public List<TRow> Rows { get; } = new List<TRow>();
}