using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Messaging
{
    /// <summary>
    /// A Notification, that mocks the one sent to Google,
    /// since it fits my needs
    /// </summary>
    public class Notification
    {
        [Key, DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        /// <summary>
        /// The title.
        /// </summary>
        [Required, Display(Name = "Title")]
        public string title { get; set; }
        /// <summary>
        /// The body.
        /// </summary>
        [Required, Display(Name = "Title")]
        public string body { get; set; }
        /// <summary>
        /// The icon.
        /// </summary>
        [Display(Name = "Icon")]
        public string icon { get; set; }
        /// <summary>
        /// The sound.
        /// </summary>
        [Display(Name = "Sound")]
        public string sound { get; set; }
        /// <summary>
        /// The tag.
        /// </summary>
        [Display(Name = "Tag")]
        public string tag { get; set; }
        /// <summary>
        /// The color.
        /// </summary>
        [Display(Name = "Color")]
        public string color { get; set; }
        /// <summary>
        /// The click action.
        /// </summary>
        [Required, Display(Name = "Click action")]
        public string click_action { get; set; }
    }
}