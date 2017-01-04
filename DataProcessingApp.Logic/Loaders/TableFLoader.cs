using System;
using System.Xml;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Logic.Loaders
{
    public class TableFLoader : BaseLoaderWithType<TableFRow>
    {
        public TableF LoadFromXML(string filename)
        {
            var table = new TableF();

            // load rows from XML file
            table.Rows = LoadDataFromXMLFile(filename);

            return table;
        }
        
        protected override TableFRow ParseRowElement(XmlNode node)
        {
            var result = new TableFRow();

            var interestReate = node.Attributes["InterestRate"].Value;
            var frequency = node.Attributes["Frequency"].Value;
            var months = node.Attributes["Months"].Value;
            var adjustmentFactor = node.Attributes["Months"].Value;

            result.InterestRate = Double.Parse(interestReate);
            result.Frequency = frequency;
            result.Months = Int32.Parse(months);
            result.AdjustmentFactor = Double.Parse(adjustmentFactor);
            return result;
        }
    }
}
