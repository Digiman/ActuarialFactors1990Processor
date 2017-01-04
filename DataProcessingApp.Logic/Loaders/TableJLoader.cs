using System;
using System.Xml;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Logic.Loaders
{
    public class TableJLoader : BaseLoaderWithType<TableJRow>
    {
        public TableJ LoadFromXML(string filename)
        {
            var table = new TableJ();

            // load rows from XML file
            table.Rows = LoadDataFromXMLFile(filename);

            return table;
        }
        
        protected override TableJRow ParseRowElement(XmlNode node)
        {
            var result = new TableJRow();

            var interestReate = node.Attributes["InterestRate"].Value;
            var frequency = node.Attributes["Frequency"].Value;
            var adjustmentFactor = node.Attributes["adjustmentFactor"].Value;

            result.InterestRate = Double.Parse(interestReate);
            result.Frequency = frequency;
            result.AdjustmentFactor = Double.Parse(adjustmentFactor);
            return result;
        }
    }
}
