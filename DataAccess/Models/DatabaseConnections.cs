namespace DataAccess.Models
{
    public class DatabaseConnections
    {
        public Connection Default { get; set; }
        public Connection SMS { get; set; }
    }

    public class Connection
    {
        public string Schema { get; set; }
        public string ConnectionString { get; set; }
    }
}
