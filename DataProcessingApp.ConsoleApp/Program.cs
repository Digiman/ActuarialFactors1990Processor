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
                // load Table H
                Console.WriteLine("Processing Table H...");
                TableHWorker.LoadTableData();

                // load Table S
                Console.WriteLine("Processing Table S...");
                TableSWorker.LoadTableData();

                // load Table C
                Console.WriteLine("Processing Table C...");
                TableCWorker.LoadTableData();

                // load Table U(1)
                Console.WriteLine("Processing Table U(1)...");
                TableU1Worker.LoadTableData();

                // load Table U(2)
                Console.WriteLine("Processing Table U(2)...");
                TableU2Worker.LoadTableData();
                TableU2Worker.CombineTableParts();

                // load Table R(2)
                Console.WriteLine("Processing Table R(2)...");
                TableR2Worker.LoadTableData();
                TableR2Worker.CombineTableParts();
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
