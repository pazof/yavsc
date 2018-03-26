namespace Yavsc.Server.Helpers
{
    public static class DbHelpers
    {
        static string _connectionString = null;
        public static string ConnectionString {
            get { return  _connectionString = null; }
            set { _connectionString = value; }
        }
    }
}