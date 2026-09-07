using System.IO;
using System.Text;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Logic.DataObjects;
using DataProcessingApp.Logic.Exporting;

namespace DataProcessingApp.Logic.Savers
{
    public class TableU1Saver : BaseSaver
    {
        public void SaveToExcel(TableU1 table, string filename)
        {
            // generate data for report to insert in Excel file
            var data = CreateDataForExcel("TableU1", table);

            // create Excel document with data
            var document = new ExcelDocument();

            // create Excel document with data
            document.CreateDocument(data, filename);
        }

        private ExcelFileData CreateDataForExcel(string tableName, TableU1 table)
        {
            var data = new ExcelFileData();

            // 1. Worksheet name.
            data.WorksheetName = tableName;

            // 2. Table headers.
            data.Headers = GetHeaders<TableU1Row>();

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

        public void SaveToTextFile(string filename, TableU1 result)
        {
            var file = new StreamWriter(filename, false, Encoding.UTF8);
            foreach (var row in result.Rows)
            {
                file.WriteLine("{0} {1} {2} {3}", row.MortalityTable, row.Age, row.AdjustedPayoutRate, row.RemainderFactor);
            }
            file.Close();
        }
    }
}
