namespace Yavsc
{
    public class DataProtectionSettings
    {
        public Dictionary<string,string> Keys { get; set; }
        public int ExpiresInHours { get; set; }
    }
}
