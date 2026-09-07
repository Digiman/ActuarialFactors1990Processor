using System.IO;
using System.Text;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Logic.DataObjects;
using DataProcessingApp.Logic.Exporting;

namespace DataProcessingApp.Logic.Savers
{
    public class TableR2Saver : BaseSaver
    {
        public void SaveToExcel(TableR2 table, string filename)
        {
            // generate data for report to insert in Excel file
            var data = CreateDataForExcel("TableR2", table);

            // create Excel document with data
            var document = new ExcelDocument();

            // create Excel document with data
            document.CreateDocument(data, filename);
        }

        private ExcelFileData CreateDataForExcel(string tableName, TableR2 table)
        {
            var data = new ExcelFileData();

            // 1. Worksheet name.
            data.WorksheetName = tableName;

            // 2. Table headers.
            data.Headers = GetHeaders<TableR2Row>();

            // 3. Data rows.
            foreach (var tableRow in table.Rows)
            {
                var row = new Row
                {
                    Values = AddRowValues(tableRow)
                };
                data.DataRows.Add(row);
            }

            return data;
        }

        public void SaveToJsonFile(string filename, TableR2 table)
        {
            var jsonData = SerializerHelper.Serialize(table.Rows, SerializeFormat.JSON);

            File.WriteAllText(filename, jsonData);
        }

        public void SaveToTextFile(string filename, TableR2 result)
        {
            var file = new StreamWriter(filename, false, Encoding.UTF8);
            foreach (var row in result.Rows)
            {
                file.WriteLine("{0} {1} {2} {3} {4}", row.MortalityTable, row.Age1, row.Age2, row.AdjustedPayoutRate, row.RemainderFactor);
            }
            file.Close();
        }
    }
}
