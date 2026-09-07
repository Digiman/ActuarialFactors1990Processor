using System.Collections.Generic;
using System.IO;
using System.Xml;
using DataProcessingApp.Core.Helpers;

namespace DataProcessingApp.Logic.Loaders
{
    public abstract class BaseLoader
    {
        protected T LoadDataFromJsonFile<T>(string filename)
        {
            var fileData = File.ReadAllText(filename);
            var result = SerializerHelper.Deserialize<T>(fileData, SerializeFormat.JSON);
            return result;
        }
    }

    public abstract class BaseLoaderWithType<TRow>
    {
        protected T LoadDataFromJsonFile<T>(string filename)
        {
            var fileData = File.ReadAllText(filename);
            var result = SerializerHelper.Deserialize<T>(fileData, SerializeFormat.JSON);
            return result;
        }

        protected List<TRow> LoadDataFromXMLFile(string filename)
        {
            var result = new List<TRow>();

            // load document
            var doc = new XmlDocument();
            doc.Load(filename);

            // parse loaded nodes
            foreach (XmlNode documentElementChildNode in doc.DocumentElement.ChildNodes)
            {
                result.Add(ParseRowElement(documentElementChildNode));
            }

            return result;
        }

        protected abstract TRow ParseRowElement(XmlNode node);
    }
}
