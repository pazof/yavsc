namespace Yavsc.Abstract.FileSystem {

  public interface IDirectoryShortInfo
  {
    string Name { get; set; }
    bool IsEmpty { get; set; }
  }
}
