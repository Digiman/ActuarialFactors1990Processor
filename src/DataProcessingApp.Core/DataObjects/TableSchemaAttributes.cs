using System;

namespace DataProcessingApp.Core.DataObjects
{
    /// <summary>
    /// Maps a row property to the exact column name in the SQL Server destination table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DbColumnAttribute : Attribute
    {
        public string Name { get; }

        public DbColumnAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Marks a double property to be rounded to the given number of digits after loading.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RoundAttribute : Attribute
    {
        public int Digits { get; }

        public RoundAttribute(int digits)
        {
            Digits = digits;
        }
    }
}
