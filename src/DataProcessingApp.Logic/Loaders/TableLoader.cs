using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using DataProcessingApp.Core.DataObjects;
using DataProcessingApp.Core.Helpers;

namespace DataProcessingApp.Logic.Loaders
{
    /// <summary>
    /// Generic loader for actuarial tables from JSON arrays or XML (SQL Server export) files.
    /// </summary>
    public class TableLoader<TRow> where TRow : new()
    {
        public List<TRow> LoadFromJson(string filename)
        {
            var fileData = File.ReadAllText(filename);
            var rows = SerializerHelper.Deserialize<List<TRow>>(fileData, SerializeFormat.JSON);
            ApplyRounding(rows);
            return rows;
        }

        public List<TRow> LoadFromXml(string filename)
        {
            var result = new List<TRow>();

            var doc = new XmlDocument();
            doc.Load(filename);

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                result.Add(ParseRowElement(node, filename));
            }

            ApplyRounding(result);
            return result;
        }

        private static TRow ParseRowElement(XmlNode node, string filename)
        {
            // build case-insensitive attribute map once per node
            var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (XmlAttribute attribute in node.Attributes)
            {
                attributes[attribute.Name] = attribute.Value;
            }

            var row = new TRow();
            foreach (var property in typeof(TRow).GetProperties())
            {
                if (!attributes.TryGetValue(property.Name, out var rawValue))
                {
                    throw new FormatException(
                        String.Format("Attribute '{0}' not found in XML element '{1}' ({2}).", property.Name, node.Name, filename));
                }

                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                property.SetValue(row, Convert.ChangeType(rawValue, targetType, CultureInfo.InvariantCulture));
            }

            return row;
        }

        private static void ApplyRounding(IEnumerable<TRow> rows)
        {
            foreach (var row in rows)
            {
                foreach (var property in typeof(TRow).GetProperties())
                {
                    var roundAttribute = (RoundAttribute)Attribute.GetCustomAttribute(property, typeof(RoundAttribute));
                    if (roundAttribute != null)
                    {
                        var value = (double)property.GetValue(row);
                        property.SetValue(row, Math.Round(value, roundAttribute.Digits));
                    }
                }
            }
        }
    }
}
