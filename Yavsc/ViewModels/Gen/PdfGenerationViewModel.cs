using System.ComponentModel.DataAnnotations;

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
    }
}