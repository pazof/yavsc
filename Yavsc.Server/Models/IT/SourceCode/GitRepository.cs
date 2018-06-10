using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models;

namespace Yavsc.Server.Models.IT.SourceCode
{
    public class GitRepositoryReference
    {

        public string Url { get; set; }
        public string Path { get; set; }
        public string Branch { get; set; }

        public string OwnerId { get; set; }
        
        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; }
    }
}