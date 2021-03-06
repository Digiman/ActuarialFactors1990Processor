using System.IO;
using System.Text;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Logic.DataObjects;
using DataProcessingApp.Logic.Exporting;

namespace DataProcessingApp.Logic.Savers
{
    public class TableSSaver : BaseSaver
    {
        public void SaveToExcel(TableS table, string filename)
        {
            // generate data for report to insert in Excel file
            var data = CreateDataForExcel("TableS", table);

            // create Excel document with data
            var document = new ExcelDocument();

            // create Excel document with data
            document.CreateDocument(data, filename);
        }

        private ExcelFileData CreateDataForExcel(string tableName, TableS table)
        {
            var data = new ExcelFileData();

            // 1. Worksheet name.
            data.WorksheetName = tableName;

            // 2. Table headers.
            data.Headers = GetHeaders<TableSRow>();

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

        public void SaveToTextFile(string filename, TableS result)
        {
            var file = new StreamWriter(filename, false, Encoding.UTF8);
            foreach (var row in result.Rows)
            {
                file.WriteLine("{0} {1} {2} {3} {4} {5}", row.MortalityTable, row.InterestRate, row.Age, row.PvAnnuity, row.PvLifeEstate, row.PvReminderInterest);
            }
            file.Close();
        }
    }
}
