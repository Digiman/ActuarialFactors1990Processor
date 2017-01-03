using DataProcessingApp.Data.Repositories;

namespace DataProcessingApp.Data
{
    public class DataFactory
    {
        private static DataFactory _dataFactory;

        public static DataFactory Instance
        {
            get { return _dataFactory ?? (_dataFactory = new DataFactory()); }
        }

        public TableHRepository GetTableHRepository(string connectionString)
        {
            return new TableHRepository(connectionString);
        }

        public TableCRepository GetTableCRepository(string connectionString)
        {
            return new TableCRepository(connectionString);
        }

        public TableSRepository GetTableSRepository(string connectionString)
        {
            return new TableSRepository(connectionString);
        }

        public TableU1Repository GetTableU1Repository(string connectionString)
        {
            return new TableU1Repository(connectionString);
        }

        public TableU2Repository GetTableU2Repository(string connectionString)
        {
            return new TableU2Repository(connectionString);
        }

        public TableR2Repository GetTableR2Repository(string connectionString)
        {
            return new TableR2Repository(connectionString);
        }
    }
}
