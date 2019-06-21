using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Relationship
{
    public partial class HyperLink
    {
        [YaStringLength(5,1024)]
        [Display(Name="HRefDisplayName", ResourceType=typeof(HyperLink), 
        Prompt="http://some.web.site")]
        public string HRef { get; set; }

        [YaStringLength(5,1024)]
        [Display(Name="MethodDisplayName", ResourceType=typeof(HyperLink), Prompt="GET")]
        public string Method { get; set; }

        [YaStringLength(5,25)]
        [Display(Name="RelDisplayName",  ResourceType=typeof(HyperLink), 
        Prompt="href")]
        public string Rel { get; set; }

        [YaStringLength(3,50)]
        [Display(Name="ContentTypeDisplayName", ResourceType=typeof(HyperLink), 
        Prompt="text/html")]
        public string ContentType { get; set; }

    }
}
