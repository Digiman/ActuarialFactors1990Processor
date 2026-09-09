using DataProcessingApp.Calculator;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DataProcessingApp.WebApi;

/// <summary>
/// Read-only HTTP access to the actuarial scenarios: scenario metadata,
/// single computations and age sweeps (the factor charts in the UI, with
/// optional overlay of all series). All computations go through the shared
/// ActuarialCalculator.
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

        var builder = WebApplication.CreateBuilder(args);

        var data = new FactorData(AppHelper.BaseDataDir);
        var calculator = new ActuarialCalculator(data);

        var app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.MapGet("/api/scenarios", () => Results.Json(ScenarioCatalog.All));

        app.MapGet("/api/series", () => Results.Json(new { series = FactorData.Series }));

        app.MapPost("/api/calc", (ScenarioRequest request) =>
        {
            try
            {
                return Results.Json(calculator.Run(request.Scenario, request.Series, RequestArguments(request)));
            }
            catch (ScenarioException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 400);
            }
        });

        app.MapPost("/api/sweep/age", (ScenarioRequest request) =>
        {
            try
            {
                return Results.Json(AgeSweep(calculator, request));
            }
            catch (ScenarioException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 400);
            }
        });

        app.MapGet("/api/mortality", (int year) =>
        {
            try
            {
                return Results.Json(new { year, rows = data.Mortality(year).Select(r => new { r.Age, r.Lx }) });
            }
            catch (ScenarioException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 400);
            }
        });

        app.Run();
    }

    /// <summary>
    /// Computes one scenario for every age 0-109 (mortality: 0-110) at fixed
    /// other inputs; every published-grid gap becomes a missing point.
    /// With compare=true and a series-based scenario, one curve per series.
    /// </summary>
    private static object AgeSweep(ActuarialCalculator calculator, ScenarioRequest request)
    {
        var descriptor = ScenarioCatalog.Find(request.Scenario);
        var ageInputs = descriptor.Inputs.Where(i => i.Name.StartsWith("age")).Select(i => i.Name).ToList();
        if (ageInputs.Count == 0)
        {
            throw new ScenarioException($"Scenario '{request.Scenario}' has no age input to sweep.");
        }

        var maxAge = descriptor.Name == "mortality" ? 110 : 109;
        var seriesList = request.Compare && descriptor.SeriesBased
            ? FactorData.Series.ToList()
            : new List<string> { string.IsNullOrEmpty(request.Series) ? calculator.DefaultSeries : request.Series };

        var curves = new List<object>();
        List<string> labels = null;

        foreach (var series in seriesList)
        {
            var points = new List<object>();
            foreach (var age in Enumerable.Range(0, maxAge + 1))
            {
                var inputs = new Dictionary<string, string>(request.Inputs ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
                foreach (var name in ageInputs)
                {
                    inputs[name] = age.ToString();
                }

                try
                {
                    var result = calculator.Run(request.Scenario, series, new ScenarioArguments(inputs));
                    labels ??= result.Values.Select(v => v.Label).ToList();
                    points.Add(new { x = age, y = result.Values.Select(v => Math.Round(v.Value, 6)).ToArray() });
                }
                catch (ScenarioException)
                {
                    // no published factor for this age: gap in the curve
                }
            }

            if (points.Count > 0)
            {
                curves.Add(new { series = descriptor.SeriesBased ? series : null, points });
            }
        }

        return new { labels = labels ?? new List<string>(), curves };
    }

    private static ScenarioArguments RequestArguments(ScenarioRequest request)
    {
        return new ScenarioArguments(request.Inputs ?? new Dictionary<string, string>());
    }
}

/// <summary>Request body of /api/calc and /api/sweep/age.</summary>
public sealed class ScenarioRequest
{
    public string Scenario { get; set; }
    public string Series { get; set; }
    public Dictionary<string, string> Inputs { get; set; }
    /// <summary>Overlay every series in one age sweep (unitrust/life-estate charts).</summary>
    public bool Compare { get; set; }
}