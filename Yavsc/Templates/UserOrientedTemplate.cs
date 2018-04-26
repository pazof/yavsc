using Yavsc.Abstract.Templates;
using Yavsc.Models;
using System.Threading.Tasks;

namespace Yavsc.Templates
{
    public class UserOrientedTemplate: Template
    {
        public ApplicationUser User { get; set; }
        
            
        public override async Task ExecuteAsync ()
        {
          throw new System.NotImplementedException();
        }

    }
}
