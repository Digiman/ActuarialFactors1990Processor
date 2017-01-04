using System;
using System.Xml;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Logic.Loaders
{
    public class TableDLoader : BaseLoaderWithType<TableDRow>
    {
        public TableD LoadFromXML(string filename)
        {
            var table = new TableD();

            // load rows from XML file
            table.Rows = LoadDataFromXMLFile(filename);

            return table;
        }
        
        protected override TableDRow ParseRowElement(XmlNode node)
        {
            var result = new TableDRow();

            var years = node.Attributes["Years"].Value;
            var payoutRate = node.Attributes["PayoutRate"].Value;
            var remainderInterest = node.Attributes["remainderInterest"].Value;

            result.Years = Int32.Parse(years);
            result.PayoutRate = Double.Parse(payoutRate);
            result.RemainderInterest = Double.Parse(remainderInterest);
            return result;
        }
    }
}
