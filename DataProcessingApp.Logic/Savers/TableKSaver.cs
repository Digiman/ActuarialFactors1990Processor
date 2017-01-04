using System.IO;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Logic.DataObjects;
using DataProcessingApp.Logic.Exporting;

namespace DataProcessingApp.Logic.Savers
{
    public class TableKSaver : BaseSaver
    {
        public void SaveToExcel(TableK table, string filename)
        {
            // generate data for report to insert in Excel file
            var data = CreateDataForExcel("TableK", table);

            // create Excel document with data
            var document = new ExcelDocument();
            
            // create Excel document with data
            document.CreateDocument(data, filename);
        }

        private ExcelFileData CreateDataForExcel(string tableName, TableK table)
        {
            var data = new ExcelFileData();

            // 1. Worksheet name.
            data.WorksheetName = tableName;

            // 2. Table headers.
            data.Headers = GetHeaders<TableKRow>();

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

        public void SaveToJsonFile(string filename, TableK table)
        {
            var jsonData = SerializerHelper.Serialize(table.Rows, SerializeFormat.JSON);

            File.WriteAllText(filename, jsonData);
        }
    }
}
