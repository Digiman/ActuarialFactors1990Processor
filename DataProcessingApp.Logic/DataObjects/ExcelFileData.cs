using System.Collections.Generic;

namespace DataProcessingApp.Logic.DataObjects
{
    public class ExcelFileData
    {
        public string WorksheetName { get; set; }
        public List<string> Headers { get; set; }
        public List<Row> DataRows { get; set; }

        public ExcelFileData()
        {
            Headers = new List<string>();
            DataRows = new List<Row>();
        }
    }

    public class Row
    {
        public List<string> Values { get; set; }
    }
}
