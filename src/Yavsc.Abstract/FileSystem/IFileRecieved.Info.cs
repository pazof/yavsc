namespace Yavsc.Abstract.FileSystem
{
    public interface IFileRecievedInfo
    {
        
        string DestDir { get; set; }

        string FileName { get; set; }

        bool Overriden { get; set; }

        bool QuotaOffensed { get; set; }
    }
}
