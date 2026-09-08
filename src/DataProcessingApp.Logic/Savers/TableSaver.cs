using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Logic.DataObjects;
using DataProcessingApp.Logic.Exporting;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataProcessingApp.Logic.Savers;

/// <summary>
/// Generic saver for actuarial tables: JSON, Excel and plain text files.
/// Column headers are taken from [DbColumn] attributes on the row type.
/// </summary>
public class TableSaver<TRow>
{
    public void SaveToJsonFile(string filename, IEnumerable<TRow> rows)
    {
        var jsonData = SerializerHelper.Serialize(rows, SerializeFormat.JSON);
        File.WriteAllText(filename, jsonData);
    }

    public void SaveToExcel(string worksheetName, string filename, IEnumerable<TRow> rows)
    {
        var data = new ExcelFileData
        {
            WorksheetName = worksheetName,
            Headers = GetColumnNames<TRow>()
        };

        foreach (var row in rows)
        {
            data.DataRows.Add(new Row
            {
                Values = GetRowValues(row)
            });
        }

        var document = new ExcelDocument();
        document.CreateDocument(data, filename);
    }

    public void SaveToTextFile(string filename, IEnumerable<TRow> rows)
    {
        using (var file = new StreamWriter(filename, false, Encoding.UTF8))
        {
            foreach (var row in rows)
            {
                file.WriteLine(string.Join(" ", GetRowValues(row)));
            }
        }
    }

    private static List<string> GetColumnNames<T>()
    {
        var result = new List<string>();
        foreach (var property in typeof(T).GetProperties())
        {
            var columnAttribute = (DbColumnAttribute)System.Attribute.GetCustomAttribute(property, typeof(DbColumnAttribute));
            result.Add(columnAttribute != null ? columnAttribute.Name : property.Name);
        }
        return result;
    }

    private static List<string> GetRowValues<TRow2>(TRow2 row)
    {
        var result = new List<string>();
        foreach (var property in typeof(TRow2).GetProperties())
        {
            var value = property.GetValue(row);
            result.Add(value != null ? value.ToString() : string.Empty);
        }
        return result;
    }
}