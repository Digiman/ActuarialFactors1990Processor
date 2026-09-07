using System;
using DataProcessingApp.ConsoleApp.Workers;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;

namespace DataProcessingApp.ConsoleApp
{
    public static class Tests
    {
        public static void LoaderTests()
        {
            Console.WriteLine("Load data from JSON files...");

            Worker<TableHRow>(TableType.TableH).LoadTableData();
            Worker<TableSRow>(TableType.TableS).LoadTableData();
            Worker<TableCRow>(TableType.TableC).LoadTableData();
            Worker<TableU1Row>(TableType.TableU1).LoadTableData();
            Worker<TableU2Row>(TableType.TableU2).LoadTableData();
            Worker<TableR2Row>(TableType.TableR2).LoadTableData();
        }

        public static void LoaderTests2()
        {
            Console.WriteLine("Load data from XML files...");

            Worker<TableKRow>(TableType.TableK).LoadTableData();
            Worker<TableJRow>(TableType.TableJ).LoadTableData();
            Worker<TableFRow>(TableType.TableF).LoadTableData();
            Worker<TableDRow>(TableType.TableD).LoadTableData();
            Worker<TableBRow>(TableType.TableB).LoadTableData();
            Worker<MortalityTableRow>(TableType.MortalityTable).LoadTableData();
        }

        public static void TextFileSaverTests()
        {
            Console.WriteLine("Save to text files...");

            Worker<TableHRow>(TableType.TableH).SaveToTextFile();
            Worker<TableSRow>(TableType.TableS).SaveToTextFile();
            Worker<TableCRow>(TableType.TableC).SaveToTextFile();
            Worker<TableU1Row>(TableType.TableU1).SaveToTextFile();
            Worker<TableU2Row>(TableType.TableU2).CombineTableParts();
            Worker<TableU2Row>(TableType.TableU2).SaveToTextFile();
            Worker<TableR2Row>(TableType.TableR2).CombineTableParts();
            Worker<TableR2Row>(TableType.TableR2).SaveToTextFile();
        }

        /// <summary>
        /// Save all data table to Excel files.
        /// </summary>
        public static void ExcelSaverTests()
        {
            Console.WriteLine("Save to Excel files...");

            Worker<TableHRow>(TableType.TableH).ExportToExcel();
            Worker<TableSRow>(TableType.TableS).ExportToExcel();
            Worker<TableCRow>(TableType.TableC).ExportToExcel();
            Worker<TableU1Row>(TableType.TableU1).ExportToExcel();
            Worker<TableU2Row>(TableType.TableU2).CombineTableParts();
            Worker<TableU2Row>(TableType.TableU2).ExportToExcel();
            Worker<TableR2Row>(TableType.TableR2).CombineTableParts();
            Worker<TableR2Row>(TableType.TableR2).ExportToExcel();

            //---------------------------------------

            Worker<TableKRow>(TableType.TableK).ExportToExcel();
            Worker<TableJRow>(TableType.TableJ).ExportToExcel();
            Worker<TableFRow>(TableType.TableF).ExportToExcel();
            Worker<TableDRow>(TableType.TableD).ExportToExcel();
            Worker<TableBRow>(TableType.TableB).ExportToExcel();
            Worker<MortalityTableRow>(TableType.MortalityTable).ExportToExcel();
        }

        public static void JsonFileSaverTests()
        {
            Console.WriteLine("Save to JSON files...");

            Worker<TableKRow>(TableType.TableK).SaveToJsonFile();
            Worker<TableJRow>(TableType.TableJ).SaveToJsonFile();
            Worker<TableFRow>(TableType.TableF).SaveToJsonFile();
            Worker<TableDRow>(TableType.TableD).SaveToJsonFile();
            Worker<TableBRow>(TableType.TableB).SaveToJsonFile();
            Worker<MortalityTableRow>(TableType.MortalityTable).SaveToJsonFile();
        }

        /// <summary>
        /// Full tests for all tables data to save in database with bulk insert.
        /// </summary>
        public static void DatabaseTests()
        {
            Console.WriteLine("Save data to database...");

            Worker<TableHRow>(TableType.TableH).SaveToDatabase();
            Worker<TableSRow>(TableType.TableS).SaveToDatabase();
            Worker<TableCRow>(TableType.TableC).SaveToDatabase();
            Worker<TableU1Row>(TableType.TableU1).SaveToDatabase();
            Worker<TableU2Row>(TableType.TableU2).CombineTableParts();
            Worker<TableU2Row>(TableType.TableU2).SaveToDatabase();
            Worker<TableR2Row>(TableType.TableR2).CombineTableParts();
            Worker<TableR2Row>(TableType.TableR2).SaveToDatabase();

            //---------------------------------------

            Worker<TableKRow>(TableType.TableK).SaveToDatabase();
            Worker<TableJRow>(TableType.TableJ).SaveToDatabase();
            Worker<TableFRow>(TableType.TableF).SaveToDatabase();
            Worker<TableDRow>(TableType.TableD).SaveToDatabase();
            Worker<TableBRow>(TableType.TableB).SaveToDatabase();
            Worker<MortalityTableRow>(TableType.MortalityTable).SaveToDatabase();
        }

        private static TableWorker<TRow> Worker<TRow>(TableType tableType) where TRow : new()
        {
            return new TableWorker<TRow>(tableType);
        }
    }
}
