namespace DataProcessingApp.Logic.Exporting
{
    using System.Collections.Generic;
    using DataProcessingApp.Logic.DataObjects;
    using Syncfusion.XlsIO;

    public class ExcelDocument
    {
        public void CreateDocument(ExcelFileData data, string filename)
        {
            // New instance of ExcelEngine is created 
            // Equivalent to launching Microsoft Excel with no workbooks open
            // Instantiate the spreadsheet creation engine
            var excelEngine = new ExcelEngine();

            // Instantiate the Excel application object
            var application = excelEngine.Excel;

            // Assigns default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            // A new workbook is created. 
            // Equivalent to creating a new workbook in Excel.
            // Create a workbook with 1 worksheet.
            IWorkbook workbook = application.Workbooks.Create(1);

            // Access first worksheet from the workbook.
            IWorksheet worksheet = workbook.Worksheets[0];

            // 1. Create worksheet with name.
            workbook.Worksheets[0].Name = data.WorksheetName;

            // 2. Create table header.
            this.CreateHeader(worksheet, data.Headers);

            // 3. Insert data.
            this.InsertDataRows(worksheet, data.DataRows);

            // Saving the workbook to disk in xlsx format
            workbook.SaveAs(filename);

            // Closing the workbook.
            workbook.Close();

            // Dispose the Excel engine
            excelEngine.Dispose();
        }

        private void CreateHeader(IWorksheet worksheet, IEnumerable<string> dataHeaders)
        {
            const int row = 1;
            var column = 1;
            foreach (var dataHeader in dataHeaders)
            {
                worksheet.Range[row, column].Text = dataHeader;
                column++;
            }
        }

        private void InsertDataRows(IWorksheet worksheet, IEnumerable<Row> dataRows)
        {
            var row = 2; // start from second row after header
            foreach (var dataRow in dataRows)
            {
                var column = 1;
                foreach (var rowValue in dataRow.Values)
                {
                    worksheet.Range[row, column].Text = rowValue;
                    column++;
                }

                row++;
            }
        }
    }
}
