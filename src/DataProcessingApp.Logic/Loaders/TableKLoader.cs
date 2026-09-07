using System;
using System.Xml;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Logic.Loaders
{
    public class TableKLoader : BaseLoaderWithType<TableKRow>
    {
        public TableK LoadFromXML(string filename)
        {
            var table = new TableK();

            // load rows from XML file
            table.Rows = LoadDataFromXMLFile(filename);

            return table;
        }

        protected override TableKRow ParseRowElement(XmlNode node)
        {
            var result = new TableKRow();

            var interestRate = node.Attributes["InterestRate"].Value;
            var frequency = node.Attributes["Frequency"].Value;
            var adjustmentFactor = node.Attributes["adjustmentFactor"].Value;

            result.InterestRate = Double.Parse(interestRate);
            result.Frequency = frequency;
            result.AdjustmentFactor = Double.Parse(adjustmentFactor);
            return result;
        }
    }
}
