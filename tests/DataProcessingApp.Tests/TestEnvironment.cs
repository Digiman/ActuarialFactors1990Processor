using System;
using System.Runtime.CompilerServices;

namespace DataProcessingApp.Tests;

/// <summary>
/// Provides an appsettings-free environment for tests: BaseDataDir is
/// supplied via the DPA_ environment variable prefix supported by AppHelper.
/// </summary>
public static class TestEnvironment
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        Environment.SetEnvironmentVariable("DPA_BaseDataDir", "testdata");
        Environment.SetEnvironmentVariable("DPA_EnvironmentName", "Test");
    }
}