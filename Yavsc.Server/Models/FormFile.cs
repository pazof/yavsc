
using System.IO;
using System.Net.Mime;

namespace Yavsc.Server.Model
{
public class FormFile
    {
        public string Name { get; set; }

        string contentDispositionString;
        public string ContentDisposition { get {
            return contentDispositionString;
        }  set {
            ContentDisposition contentDisposition = new ContentDisposition(value);
            Name = contentDisposition.FileName;
            contentDispositionString = value;
        }  }

        public string ContentType { get; set; }

        public string FilePath { get; set; }

        public Stream Stream { get; set; }
    }
}