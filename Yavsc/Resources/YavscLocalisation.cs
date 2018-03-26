using System.IO;
using System.Resources;

namespace Yavsc.Resources
{

    ///<summary>
    /// Makes possible the code injection
    ///</summary>
    public class YavscLocalisation : ResourceSet
    {
        public YavscLocalisation(string path) : base(path)
        {
        }
        public YavscLocalisation(Stream stream) : base(stream)
        {
        }

    }
}
