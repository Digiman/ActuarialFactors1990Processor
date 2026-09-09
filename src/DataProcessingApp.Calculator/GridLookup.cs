using System;
using System.Collections.Generic;
using System.Linq;

namespace DataProcessingApp.Calculator;

/// <summary>
/// Exact-cell lookups over the published tables. IRS factors are only defined
/// on the published grids (0.2% rate steps, integer ages), so lookups match
/// exactly and every failure names the offending value and the actual grid.
/// </summary>
public static class GridLookup
{
    public const double Tolerance = 1e-9;

    public static double RoundRate(double rate)
    {
        return Math.Round(rate, 1);
    }

    public static bool Same(double left, double right)
    {
        return Math.Abs(left - right) < Tolerance;
    }

    public static TRow FindRow<TRow>(
        IReadOnlyList<TRow> rows, Func<TRow, bool> match, string missing, string tableLabel)
    {
        var row = rows.FirstOrDefault(match);
        if (row == null)
        {
            throw new ScenarioException($"{missing} in {tableLabel}.");
        }

        return row;
    }

    /// <summary>Validates an integer input against its published range.</summary>
    public static int ValidateRange(int value, int min, int max, string name)
    {
        if (value < min || value > max)
        {
            throw new ScenarioException($"{name} {value} is outside the published range {min}-{max}.");
        }

        return value;
    }

    /// <summary>
    /// Human-readable label for a data-derived rate grid, e.g. "0.2-20% in 0.2 steps".
    /// </summary>
    public static string GridLabel(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
        {
            return "empty grid";
        }

        return $"{values[0]}-{values[values.Count - 1]}% in 0.2 steps";
    }

    /// <summary>Distinct sorted rate values in the rows (the actual published grid).</summary>
    public static List<double> DistinctRates<TRow>(IReadOnlyList<TRow> rows, Func<TRow, double> rate)
    {
        return rows.Select(rate).Distinct().OrderBy(v => v).ToList();
    }

    public static string AgeGridLabel => "ages 0-109";
}