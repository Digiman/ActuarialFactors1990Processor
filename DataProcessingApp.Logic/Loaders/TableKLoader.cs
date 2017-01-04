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

            var interestReate = node.Attributes["InterestRate"].Value;
            var frequency = node.Attributes["Frequency"].Value;
            var adjustmentFactor = node.Attributes["adjustmentFactor"].Value;

            result.InterestRate = Double.Parse(interestReate);
            result.Frequency = frequency;
            result.AdjustmentFactor = Double.Parse(adjustmentFactor);
            return result;
        }

        /*private List<TableKRow> LoadDataFromXMLFile2(string filename)
        {
            var result = new List<TableKRow>();

            // load data from file bu reading
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            XmlReader reader = XmlReader.Create(fs);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "dbo.tblK")
                        {
                            result.Add(ParseRowElement(reader));
                        }
                        break;
                }
            }
            fs.Close();

            return result;
        }

        private TableKRow ParseRowElement(XmlReader reader)
        {
            var result = new TableKRow();
            
            var interestReate = reader.GetAttribute("InterestRate");
            var frequency = reader.GetAttribute("Frequency");
            var adjustmentFactor = reader.GetAttribute("adjustmentFactor");

            result.InterestRate = Double.Parse(interestReate);
            result.Frequency = frequency;
            result.AdjustmentFactor = Double.Parse(adjustmentFactor);
            return result;
        }*/
    }
}
