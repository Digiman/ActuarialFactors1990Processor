using System;
using System.Diagnostics;

namespace DataProcessingApp.ConsoleApp
{
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // loaders tests
                //Tests.LoaderTests();

                // save to text files
                //Tests.TextFileSaverTests();

                // save to excel
                //Tests.ExcelSaverTests();

                // load data from files and save to database
                var databaseTime = ExecuteWithTiming(Tests.DatabaseTests);
                Console.WriteLine("Database copy: {0} ms", databaseTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        static long ExecuteWithTiming(Action action)
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
