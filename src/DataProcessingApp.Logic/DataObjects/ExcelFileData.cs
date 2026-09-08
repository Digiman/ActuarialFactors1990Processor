using System.Collections.Generic;

namespace DataProcessingApp.Logic.DataObjects;

public sealed class ExcelFileData
{
    public string WorksheetName { get; set; }

    public List<string> Headers { get; set; }

    public List<Row> DataRows { get; set; }

    public ExcelFileData()
    {
        Headers = [];
        DataRows = [];
    }
}

public sealed class Row
{
    public List<string> Values { get; set; }
}