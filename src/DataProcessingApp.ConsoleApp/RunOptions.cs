using DataProcessingApp.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataProcessingApp.ConsoleApp;

/// <summary>
/// Runtime options for one workflow execution: optional series/table filters
/// and dry-run mode. Parsed and validated from the command line settings.
/// </summary>
public sealed class RunOptions
{
    public static readonly RunOptions Default = new(Array.Empty<string>(), Array.Empty<string>(), false);

    public RunOptions(string[] seriesFilter, string[] tablesFilter, bool dryRun)
    {
        SeriesFilter = SplitList(seriesFilter);
        TablesFilter = SplitList(tablesFilter).Select(ParseTable).Distinct().ToArray();
        DryRun = dryRun;
    }

    public string[] SeriesFilter { get; }

    /// <summary>Parsed <see cref="TableType"/> values to restrict processing to; empty means all.</summary>
    public TableType[] TablesFilter { get; }

    public bool DryRun { get; }

    public bool Includes(TableType tableType)
    {
        return TablesFilter.Length == 0 || TablesFilter.Contains(tableType);
    }

    /// <summary>
    /// List options accept both repeated values (--table S --table K) and
    /// comma-separated values (--table S,K).
    /// </summary>
    private static string[] SplitList(string[] values)
    {
        return values
            .SelectMany(v => v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToArray();
    }

    /// <summary>
    /// Maps user-supplied table names ("S", "TableS", "MortalityTable") to
    /// <see cref="TableType"/> values; throws with the valid names otherwise.
    /// </summary>
    public static TableType ParseTable(string name)
    {
        var normalized = name.Trim().Replace(" ", String.Empty);

        if (String.Equals(normalized, "Mortality", StringComparison.OrdinalIgnoreCase) ||
            String.Equals(normalized, "MortalityTable", StringComparison.OrdinalIgnoreCase))
        {
            return TableType.MortalityTable;
        }

        var enumName = normalized.StartsWith("Table", StringComparison.OrdinalIgnoreCase)
            ? normalized
            : $"Table{normalized}";

        if (Enum.TryParse<TableType>(enumName, ignoreCase: true, out var tableType) && Enum.IsDefined(tableType))
        {
            return tableType;
        }

        throw new ArgumentException(
            $"Unknown table '{name}'. Valid names: {ValidTableNames()}");
    }

    private static string ValidTableNames()
    {
        var tableNames = Enum.GetNames<TableType>()
            .Where(n => n != nameof(TableType.MortalityTable))
            .Select(n => n.Replace("Table", String.Empty))
            .ToList();
        tableNames.Add(nameof(TableType.MortalityTable));
        return String.Join(", ", tableNames);
    }
}