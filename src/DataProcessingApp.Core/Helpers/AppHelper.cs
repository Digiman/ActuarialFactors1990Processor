using System;
using Microsoft.Extensions.Configuration;

namespace DataProcessingApp.Core.Helpers
{
    public static class AppHelper
    {
        private static readonly IConfiguration Config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables(prefix: "DPA_")
            .Build();

        public static string EnvironmentName
        {
            get { return Config["EnvironmentName"] ?? "Local"; }
        }

        public static string BaseDataDir
        {
            get { return Config["BaseDataDir"]; }
        }

        public static string DatabaseConnectionString
        {
            get { return Config.GetConnectionString(EnvironmentName); }
        }
    }
}
