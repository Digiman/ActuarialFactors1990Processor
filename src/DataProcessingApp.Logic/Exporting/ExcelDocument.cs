using ClosedXML.Excel;
using DataProcessingApp.Logic.DataObjects;
using System.Collections.Generic;

namespace DataProcessingApp.Logic.Exporting;

public class ExcelDocument
{
    public void CreateDocument(ExcelFileData data, string filename)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add(data.WorksheetName);

            CreateHeader(worksheet, data.Headers);
            InsertDataRows(worksheet, data.DataRows);

            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Columns().AdjustToContents(1, 50);

            workbook.SaveAs(filename);
        }
    }

    private void CreateHeader(IXLWorksheet worksheet, IEnumerable<string> dataHeaders)
    {
        const int row = 1;
        var column = 1;
        foreach (var dataHeader in dataHeaders)
        {
            worksheet.Cell(row, column).Value = dataHeader;
            column++;
        }
    }

    private void InsertDataRows(IXLWorksheet worksheet, IEnumerable<Row> dataRows)
    {
        var row = 2; // start from second row after header
        foreach (var dataRow in dataRows)
        {
            var column = 1;
            foreach (var rowValue in dataRow.Values)
            {
                if (double.TryParse(rowValue, out var numericValue))
                {
                    worksheet.Cell(row, column).Value = numericValue;
                }
                else
                {
                    worksheet.Cell(row, column).Value = rowValue;
                }

                column++;
            }

            row++;
        }
    }
}