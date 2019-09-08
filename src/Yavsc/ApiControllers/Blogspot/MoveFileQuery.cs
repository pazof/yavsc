using Yavsc.Attributes.Validation;
namespace Yavsc.Models.FileSystem
{
        public class RenameFileQuery {
            [ValidRemoteUserFilePath]
            [YaStringLength(1, 512)]
            public string id { get; set; } 

            [YaStringLength(0, 512)]
            [ValidRemoteUserFilePath]
            public string to { get; set; } 
        }
        public class MoveFileQuery {
            [ValidRemoteUserFilePath]
            [YaStringLength(1, 512)]
            public string id { get; set; } 

            [YaStringLength(0, 512)]
            [ValidRemoteUserFilePath]
            public string to { get; set; } 
        }

}
