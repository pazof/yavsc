using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Gen
{
    public class PdfGenerationViewModel
    {
        
        [YaRequired]
        public string TeXSource { get; set; }
        [YaRequired]
        public string BaseFileName { get; set; }
        [YaRequired]
        public string DestDir { get; set; }
        public bool Generated { get; set; }
        public HtmlString GenerationErrorMessage { get; set; }
        public string Temp { get; set; }
    }
}
