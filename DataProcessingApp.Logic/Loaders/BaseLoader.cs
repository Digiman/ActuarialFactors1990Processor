using System.IO;
using DataProcessingApp.Core.Helpers;

namespace DataProcessingApp.Logic.Loaders
{
    public class BaseLoader
    {
        protected T LoadDataFromJsonFile<T>(string filename)
        {
            var fileData = File.ReadAllText(filename);
            var result = SerializerHelper.Deserialize<T>(fileData, SerializeFormat.JSON);
            return result;
        }
    }
}
