using System;
using DataProcessingApp.ConsoleApp.Workers;

namespace DataProcessingApp.ConsoleApp
{
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TableHWorker.LoadTableHData();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }
}
