using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataProcessingApp.Core.DataObjects;

namespace DataProcessingApp.Data.Helpers
{
    public static class DataTableHelper
    {
        /// <summary>
        /// Creates a DataTable from table rows. Column names come from [DbColumn]
        /// attributes on the row type (property name as fallback); column order
        /// follows property declaration order.
        /// </summary>
        public static DataTable CreateDataTable<TRow>(IEnumerable<TRow> rows)
        {
            var dataTable = new DataTable();
            var properties = typeof(TRow).GetProperties();

            foreach (var property in properties)
            {
                dataTable.Columns.Add(GetColumnName(property), Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            foreach (var row in rows)
            {
                dataTable.Rows.Add(properties.Select(property => property.GetValue(row) ?? DBNull.Value).ToArray());
            }

            return dataTable;
        }

        private static string GetColumnName(System.Reflection.PropertyInfo property)
        {
            var columnAttribute = (DbColumnAttribute)Attribute.GetCustomAttribute(property, typeof(DbColumnAttribute));
            return columnAttribute != null ? columnAttribute.Name : property.Name;
        }
    }
}
