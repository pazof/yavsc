using System.Collections.Generic;

namespace Yavsc.Abstract.IT
{
    public interface IProject
    {
         long Id { get; set ; }
         string OwnerId { get; set; }        
         string Name { get; set; }
         string Version { get; set; }

         IEnumerable<string> GetConfigurations();
    }
}