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
        /// Run processing.
        /// </summary>
        /// <param name="args">Arguments from command line.</param>
        static void Main(string[] args)
        {
            CultureFix();

            try
            {
                // loaders tests
                //LoadData();

                // save to text files
                //SaveToTextFiles();

                // save to excel
                //SaveToExcelFiles();

                // save to json tests
                SaveToJson();

                // load data from files and save to database
                //DatabaseInsert();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static void CultureFix()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        private static void SaveToExcelFiles()
        {
            var time = ExecuteWithTiming(Tests.ExcelSaverTests);
            Console.WriteLine("Saving to Excel some tables: {0} ms", time);
        }

        private static void SaveToTextFiles()
        {
            var time = ExecuteWithTiming(Tests.TextFileSaverTests);
            Console.WriteLine("Saving to text some tables: {0} ms", time);
        }

        private static void LoadData()
        {
            var loadersTime = ExecuteWithTiming(Tests.LoaderTests);
            Console.WriteLine("Loading tables time: {0} ms", loadersTime);

            var loadersTime2 = ExecuteWithTiming(Tests.LoaderTests2);
            Console.WriteLine("Loading tables time: {0} ms", loadersTime2);
        }

        private static void DatabaseInsert()
        {
            var databaseTime = ExecuteWithTiming(Tests.DatabaseTests);
            Console.WriteLine("Database copy: {0} ms", databaseTime);
        }

        private static void SaveToJson()
        {
            var time = ExecuteWithTiming(Tests.JsonFileSaverTests);
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
