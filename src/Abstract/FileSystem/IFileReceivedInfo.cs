namespace Yavsc.Abstract.FileSystem
{
    public interface IFileReceivedInfo
    {
        
        string DestDir { get; set; }

        string FileName { get; set; }

        bool Overridden { get; set; }

        bool QuotaOffense { get; set; }
    }
}
