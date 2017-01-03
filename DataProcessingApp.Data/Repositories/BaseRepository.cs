namespace DataProcessingApp.Data.Repositories
{
    public class BaseRepository
    {
        protected string ConnectionString { get; set; }

        public BaseRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}