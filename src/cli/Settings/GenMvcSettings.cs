namespace cli.Settings
{
    public class GenMvcSettings
    {
        public string NameSpace { get; set; }
        public string ModelFullName { get; set; }
        public string ControllerName { get; set; }
        public string AppBase { get; set; }
        public string RelativePath { get; set; }
        public string DbContextFullName { get; set; }
        
        // FIXME this appears to be dropped soon ... as all configuration should be concerned
        public string ConfigurationName { get; set; } = "Development";
        
    }
}