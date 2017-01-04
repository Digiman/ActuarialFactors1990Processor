﻿using System;
using System.Globalization;
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
    }
}
