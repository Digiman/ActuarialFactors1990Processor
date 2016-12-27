using System.Collections.Generic;

namespace DataProcessingApp.Logic.Savers
{
    public class BaseSaver
    {
        protected List<string> GetHeaders<T>()
        {
            var result = new List<string>();
            var properties = typeof(T).GetProperties();
            foreach (var propertyInfo in properties)
            {
                result.Add(propertyInfo.Name);
            }
            return result;
        }

        protected List<string> AddRowValues<T>(T tableRow)
        {
            var result = new List<string>();
            var properties = tableRow.GetType().GetProperties();
            foreach (var propertyInfo in properties)
            {
                result.Add(propertyInfo.GetValue(tableRow).ToString());
            }
            return result;
        }
    }
}
