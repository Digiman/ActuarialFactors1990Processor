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
                // loaders tests
                LoaderTests();

                // save to text files
                TextFileSaverTests();

                // save to excel
                ExcelSaverTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
        
        private static void LoaderTests()
        {
            Console.WriteLine("Load data from JSON files...");

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

            // load Table R(2)
            Console.WriteLine("Processing Table R(2)...");
            TableR2Worker.LoadTableData();
        }

        private static void TextFileSaverTests()
        {
            Console.WriteLine("Save to text files...");

            // load Table H
            Console.WriteLine("Processing Table H...");
            TableHWorker.SaveToTextFileFile();

            // load Table S
            Console.WriteLine("Processing Table S...");
            TableSWorker.SaveToTextFileFile();

            // load Table C
            Console.WriteLine("Processing Table C...");
            TableCWorker.SaveToTextFileFile();

            // load Table U(1)
            Console.WriteLine("Processing Table U(1)...");
            TableU1Worker.SaveToTextFileFile();

            // load Table U(2)
            Console.WriteLine("Processing Table U(2)...");
            TableU2Worker.CombineTableParts();
            TableU2Worker.SaveToTextFileFile();

            // load Table R(2)
            Console.WriteLine("Processing Table R(2)...");
            TableR2Worker.CombineTableParts();
            TableR2Worker.SaveToTextFileFile();
        }

        private static void ExcelSaverTests()
        {
            Console.WriteLine("Save to Excel files...");

            // load Table H
            Console.WriteLine("Processing Table H...");
            TableHWorker.ExportToExcel();

            // load Table S
            Console.WriteLine("Processing Table S...");
            TableSWorker.ExportToExcel();

            // load Table C
            Console.WriteLine("Processing Table C...");
            TableCWorker.ExportToExcel();

            // load Table U(1)
            Console.WriteLine("Processing Table U(1)...");
            TableU1Worker.ExportToExcel();

            // load Table U(2)
            Console.WriteLine("Processing Table U(2)...");
            TableU2Worker.CombineTableParts();
            TableU2Worker.ExportToExcel();

            // load Table R(2)
            Console.WriteLine("Processing Table R(2)...");
            TableR2Worker.CombineTableParts();
            TableR2Worker.ExportToExcel();
        }
    }
}
