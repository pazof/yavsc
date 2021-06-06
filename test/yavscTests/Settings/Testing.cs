namespace yavscTests.Settings
{
    public class PasswordCreds
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class TestingSetup
    {
        public DbConnectionSettings ConnectionStrings { get; set; }

        public PasswordCreds ValidCreds
        {
            get; set;
        }
        public PasswordCreds InvalidCreds
        {
            get; set;
        }
        public string YavscWebPath {get; set; }
    }
}

