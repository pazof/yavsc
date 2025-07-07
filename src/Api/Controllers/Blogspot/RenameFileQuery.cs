using Yavsc.Attributes.Validation;
namespace Yavsc.Models.FileSystem
{
    public class RenameFileQuery
    {
        [ValidRemoteUserFilePath]
        [YaStringLength(1, 512)]
        public required string Id { get; set; }

        [YaStringLength(0, 512)]
        [ValidRemoteUserFilePath]
        public required string To { get; set; }
    }

}
