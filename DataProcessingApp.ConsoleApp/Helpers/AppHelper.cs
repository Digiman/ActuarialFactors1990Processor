using System.Configuration;

namespace DataProcessingApp.ConsoleApp.Helpers
{
    public static class AppHelper
    {
        public static string DatabaseConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; }
        }
    }
}
