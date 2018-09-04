namespace test.Settings
{
    public abstract class DbConnectionSettings
    {
            public string Database { get; set; }
            public string Server { get; set; }
            public int  Port { get; set; }
            public string  Username { get; set; }

            public string ConnectionString => $"Database={Database};Server={Server};Port={Port};Username={Username};Password={Password};";
            
            public string  Password { get; set; }
    }

    public class DevConnectionSettings : DbConnectionSettings
    {

    }
    public class TestConnectionSettings : DbConnectionSettings
    {
        
    }
}