using System;
using System.Xml;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Logic.Loaders
{
    public class TableBLoader : BaseLoaderWithType<TableBRow>
    {
        public TableB LoadFromXML(string filename)
        {
            var table = new TableB();

            // load rows from XML file
            table.Rows = LoadDataFromXMLFile(filename);

            return table;
        }

        protected override TableBRow ParseRowElement(XmlNode node)
        {
            var result = new TableBRow();

            var years = node.Attributes["Years"].Value;
            var rate = node.Attributes["Rate"].Value;
            var pvAnnuity = node.Attributes["pvAnnuity"].Value;
            var pvIncomeInterest = node.Attributes["pvIncomeInterest"].Value;
            var pvRemainderInterest = node.Attributes["pvRemainderInterest"].Value;

            result.Years = Double.Parse(years);
            result.Rate = Double.Parse(rate);
            result.PvAnnuity = Double.Parse(pvAnnuity);
            result.PvIncomeInterest = Double.Parse(pvIncomeInterest);
            result.PvRemainderInterest = Double.Parse(pvRemainderInterest);
            return result;
        }
    }
}
