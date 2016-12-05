using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Forms
{
    using Interfaces;
    
    public class Form
    {
        [Key]
        public string Id {get; set;}

        public string Summary { get; set; } 
        public List<IFormNode> Content { get; set; }
    }
}