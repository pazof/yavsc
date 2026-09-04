using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Workflow
{
    /// <summary>
    /// Supported country of exercise for performer profile validations.
    /// </summary>
    public class Country
    {
        [Key]
        [MaxLength(2)]
        [MinLength(2)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string DisplayName { get; set; } = string.Empty;
    }
}