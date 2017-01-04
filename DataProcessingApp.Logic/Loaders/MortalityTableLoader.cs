using System;
using System.Xml;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Logic.Loaders
{
    public class MortalityTableLoader : BaseLoaderWithType<MortalityTableRow>
    {
        public MortalityTable LoadFromXML(string filename)
        {
            var table = new MortalityTable();

            // load rows from XML file
            table.Rows = LoadDataFromXMLFile(filename);

            return table;
        }

        protected override MortalityTableRow ParseRowElement(XmlNode node)
        {
            var result = new MortalityTableRow();

            var year = node.Attributes["Year"].Value;
            var age = node.Attributes["Age"].Value;
            var lx = node.Attributes["lx"].Value;

            result.Year = Int32.Parse(year);
            result.Age = Int32.Parse(age);
            result.Lx = Int32.Parse(lx);

            return result;
        }
    }
}
