using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Mvc.Rendering;

namespace Yavsc.ViewModels.Gen
{
    public class PdfGenerationViewModel
    {
        
        [Required]
        public string TeXSource { get; set; }
        [Required]
        public string BaseFileName { get; set; }
        [Required]
        public string DestDir { get; set; }
        public bool Generated { get; set; }
        public HtmlString GenerationErrorMessage { get; set; }
    }
}