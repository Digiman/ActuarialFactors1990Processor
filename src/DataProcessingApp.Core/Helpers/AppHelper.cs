using Microsoft.Extensions.Configuration;
using System;

namespace DataProcessingApp.Core.Helpers;

public static class AppHelper
{
    private static readonly IConfiguration Config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true)
        .AddEnvironmentVariables(prefix: "DPA_")
        .Build();

    public static string EnvironmentName => Config["EnvironmentName"] ?? "Local";

    public static string BaseDataDir => Config["BaseDataDir"];

    /// <summary>
    /// Folder with the SQL-export XML files; falls back to BaseDataDir when not set.
    /// </summary>
    public static string XmlDataDir => Config["XmlDataDir"] is { Length: > 0 } dir ? dir : BaseDataDir;

    public static string DatabaseConnectionString => Config.GetConnectionString(EnvironmentName);
}