using System;
using System.Configuration;

namespace DataProcessingApp.ConsoleApp.Helpers
{
    public static class AppHelper
    {
        public static string EnvironmentName
        {
            get { return ConfigurationManager.AppSettings["EnvironmentName"]; }
        }

        public static string DatabaseConnectionString
        {
            get
            {
                var connectionStringName = String.Format("ConnectionString-{0}", EnvironmentName);
                return ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            }
        }
    }
}
