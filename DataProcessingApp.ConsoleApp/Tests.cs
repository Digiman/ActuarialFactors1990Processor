using System;
using DataProcessingApp.ConsoleApp.Workers;

namespace DataProcessingApp.ConsoleApp
{
    public static class Tests
    {
        public static void LoaderTests()
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

        public static void LoaderTests2()
        {
            Console.WriteLine("Load data from XML files...");

            // load Table K
            Console.WriteLine("Processing Table K...");
            TableKWorker.LoadTableData();

            // load Table J
            Console.WriteLine("Processing Table J...");
            TableJWorker.LoadTableData();

            // load Table F
            Console.WriteLine("Processing Table F...");
            TableFWorker.LoadTableData();

            // load Table D
            Console.WriteLine("Processing Table D...");
            TableDWorker.LoadTableData();

            // load Table B
            Console.WriteLine("Processing Table B...");
            TableBWorker.LoadTableData();

            // load Mortality Table B
            Console.WriteLine("Processing Mortality Table...");
            MortalityTableWorker.LoadTableData();
        }

        public static void TextFileSaverTests()
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

        /// <summary>
        /// Save all data table to Excel files.
        /// </summary>
        public static void ExcelSaverTests()
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

            //---------------------------------------

            // load Table K
            Console.WriteLine("Processing Table K...");
            TableKWorker.ExportToExcel();

            // load Table J
            Console.WriteLine("Processing Table J...");
            TableJWorker.ExportToExcel();

            // load Table F
            Console.WriteLine("Processing Table F...");
            TableFWorker.ExportToExcel();

            // load Table D
            Console.WriteLine("Processing Table D...");
            TableDWorker.ExportToExcel();

            // load Table B
            Console.WriteLine("Processing Table B...");
            TableBWorker.ExportToExcel();

            // load Mortality Table B
            Console.WriteLine("Processing Mortality Table...");
            MortalityTableWorker.ExportToExcel();
        }

        public static void JsonFileSaverTests()
        {
            Console.WriteLine("Save to JSON files...");

            // load Table K
            Console.WriteLine("Processing Table K...");
            TableKWorker.SaveToJsonFile();

            // load Table J
            Console.WriteLine("Processing Table J...");
            TableJWorker.SaveToJsonFile();

            // load Table F
            Console.WriteLine("Processing Table F...");
            TableFWorker.SaveToJsonFile();

            // load Table D
            Console.WriteLine("Processing Table D...");
            TableDWorker.SaveToJsonFile();

            // load Table B
            Console.WriteLine("Processing Table B...");
            TableBWorker.SaveToJsonFile();

            // load Mortality Table B
            Console.WriteLine("Processing Mortality Table...");
            MortalityTableWorker.SaveToJsonFile();
        }

        /// <summary>
        /// Full tests for all tables data to save in database with bulk insert.
        /// </summary>
        public static void DatabaseTests()
        {
            Console.WriteLine("Save data to database...");

            // load Table H
            Console.WriteLine("Processing Table H...");
            TableHWorker.SaveToDatabase();

            // load Table S
            Console.WriteLine("Processing Table S...");
            TableSWorker.SaveToDatabase();

            // load Table C
            Console.WriteLine("Processing Table C...");
            TableCWorker.SaveToDatabase();

            // load Table U(1)
            Console.WriteLine("Processing Table U(1)...");
            TableU1Worker.SaveToDatabase();

            // load Table U(2)
            Console.WriteLine("Processing Table U(2)...");
            TableU2Worker.CombineTableParts();
            TableU2Worker.SaveToDatabase();

            // load Table R(2)
            Console.WriteLine("Processing Table R(2)...");
            TableR2Worker.CombineTableParts();
            TableR2Worker.SaveToDatabase();

            //---------------------------------------

            // load Table K
            Console.WriteLine("Processing Table K...");
            TableKWorker.SaveToDatabase();

            // load Table J
            Console.WriteLine("Processing Table J...");
            TableJWorker.SaveToDatabase();

            // load Table F
            Console.WriteLine("Processing Table F...");
            TableFWorker.SaveToDatabase();

            // load Table D
            Console.WriteLine("Processing Table D...");
            TableDWorker.SaveToDatabase();

            // load Table B
            Console.WriteLine("Processing Table B...");
            TableBWorker.SaveToDatabase();

            // load Mortality Table B
            Console.WriteLine("Processing Mortality Table...");
            MortalityTableWorker.SaveToDatabase();
        }
    }
}
