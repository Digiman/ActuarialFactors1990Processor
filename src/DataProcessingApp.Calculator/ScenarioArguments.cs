using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DataProcessingApp.Calculator;

/// <summary>
/// Named, validated scenario inputs. Unknown names are rejected immediately
/// (catches typos like --payout on a scenario that takes --rate); missing
/// inputs fall back to the descriptor defaults.
/// </summary>
public sealed class ScenarioArguments
{
    private readonly Dictionary<string, string> _values;

    public ScenarioArguments(IDictionary<string, string> values)
    {
        _values = new Dictionary<string, string>(values ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
    }

    public string GetString(string name)
    {
        return _values.TryGetValue(name, out var value) ? value : null;
    }

    public double GetRate(string name, string def)
    {
        var raw = _values.TryGetValue(name, out var value) ? value : def;
        if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new ScenarioException($"{name} '{raw}' is not a number (use e.g. 5.2 for 5.2%).");
        }

        return GridLookup.RoundRate(parsed);
    }

    public double GetRate(string name, ScenarioInput input)
    {
        return GetRate(name, input?.Default);
    }

    public int GetInt(string name, string def)
    {
        var raw = _values.TryGetValue(name, out var value) ? value : def;
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new ScenarioException($"{name} '{raw}' is not an integer.");
        }

        return parsed;
    }

    public int GetInt(string name, ScenarioInput input)
    {
        return GetInt(name, input?.Default);
    }

    public string GetChoice(string name, ScenarioInput input)
    {
        var raw = _values.TryGetValue(name, out var value) ? value : input.Default;
        var match = input.Choices.FirstOrDefault(c => string.Equals(c, raw, StringComparison.OrdinalIgnoreCase));
        if (match == null)
        {
            throw new ScenarioException($"{name} '{raw}' is invalid; valid values: {string.Join(", ", input.Choices)}.");
        }

        return match;
    }
}