using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace DataProcessingApp.ConsoleApp
{
    /// <summary>
    /// Main class for simple console application.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Run processing. Optional argument selects the workflow:
        /// load | json (default) | text | excel | database | all.
        /// </summary>
        /// <param name="args">Arguments from command line.</param>
        static void Main(string[] args)
        {
            CultureFix();

            var workflow = args.Length > 0 ? args[0].ToLowerInvariant() : "json";

            try
            {
                switch (workflow)
                {
                    case "load":
                        LoadData();
                        break;
                    case "json":
                        SaveToJson();
                        break;
                    case "text":
                        SaveToTextFiles();
                        break;
                    case "excel":
                        SaveToExcelFiles();
                        break;
                    case "database":
                        DatabaseInsert();
                        break;
                    case "all":
                        LoadData();
                        SaveToJson();
                        SaveToTextFiles();
                        SaveToExcelFiles();
                        break;
                    default:
                        Console.WriteLine("Unknown workflow '{0}'. Use: load | json | text | excel | database | all", workflow);
                        return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                throw;
            }

            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("Press any key...");
                Console.ReadKey();
            }
        }

        private static void CultureFix()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        private static void SaveToExcelFiles()
        {
            var time = ExecuteWithTiming(Workflows.ExcelSaverTests);
            Console.WriteLine("Saving to Excel some tables: {0} ms", time);
        }

        private static void SaveToTextFiles()
        {
            var time = ExecuteWithTiming(Workflows.TextFileSaverTests);
            Console.WriteLine("Saving to text some tables: {0} ms", time);
        }

        private static void LoadData()
        {
            var loadersTime = ExecuteWithTiming(Workflows.LoaderTests);
            Console.WriteLine("Loading tables time: {0} ms", loadersTime);

            var loadersTime2 = ExecuteWithTiming(Workflows.LoaderTests2);
            Console.WriteLine("Loading tables time: {0} ms", loadersTime2);
        }

        private static void DatabaseInsert()
        {
            var databaseTime = ExecuteWithTiming(Workflows.DatabaseTests);
            Console.WriteLine("Database copy: {0} ms", databaseTime);
        }

        private static void SaveToJson()
        {
            var time = ExecuteWithTiming(Workflows.JsonFileSaverTests);
            Console.WriteLine("Saving to JSON some tables: {0} ms", time);
        }

        private static long ExecuteWithTiming(Action action)
        {
            var timer = new Stopwatch();

            timer.Start();

            // so some action
            action();

            timer.Stop();

            return timer.ElapsedMilliseconds;
        }
    }
}
