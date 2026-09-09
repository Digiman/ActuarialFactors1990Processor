using System;

namespace DataProcessingApp.Calculator;

/// <summary>
/// Raised when a scenario cannot be computed: unknown scenario name, input
/// outside the published grid, missing table cell, or bad series name.
/// The message is user-facing: it always names the scenario, the offending
/// input and the valid values.
/// </summary>
public class ScenarioException : Exception
{
    public ScenarioException(string message) : base(message)
    {
    }
}