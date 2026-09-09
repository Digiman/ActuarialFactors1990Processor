using System.Collections.Generic;

namespace DataProcessingApp.Calculator;

/// <summary>
/// One labeled value of a scenario result (renders as a table row in the CLI
/// and as a JSON pair in the web API).
/// </summary>
public sealed class ScenarioValue
{
    public string Label { get; set; }
    public double Value { get; set; }
    public string Note { get; set; }

    public ScenarioValue(string label, double value, string note = null)
    {
        Label = label;
        Value = value;
        Note = note;
    }
}

/// <summary>
/// Uniform result of a scenario computation: metadata plus labeled values,
/// so every consumer (CLI, web API, tests) renders results the same way.
/// </summary>
public sealed class ScenarioResult
{
    public string Scenario { get; set; }
    public string Title { get; set; }
    public string Purpose { get; set; }
    public string Citation { get; set; }
    public string Series { get; set; }
    public int CensusYear { get; set; }
    public List<ScenarioValue> Values { get; } = new List<ScenarioValue>();

    public ScenarioValue Add(string label, double value, string note = null)
    {
        var entry = new ScenarioValue(label, value, note);
        Values.Add(entry);
        return entry;
    }
}