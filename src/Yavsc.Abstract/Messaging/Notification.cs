using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Abstract.Models.Messaging
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
        [Required, Display(Name = "Titre")]
        [StringLength(1024)]
        public string title { get; set; }
        /// <summary>
        /// The body.
        /// </summary>
        [StringLength(512)]
        [Required, Display(Name = "Corps")]
        public string body { get; set; }
        /// <summary>
        /// The icon.
        /// </summary>
        [StringLength(512)]
        [Display(Name = "Icône")]
        public string? icon { get; set; } 
        /// <summary>
        /// The sound.
        /// </summary>
        [StringLength(512)]
        [Display(Name = "Son")]
        public string? sound { get; set; }
        /// <summary>
        /// The tag.
        /// </summary>
        [StringLength(512)]
        [Display(Name = "Tag")]
        public string? tag { get; set; }
        /// <summary>
        /// The color.
        /// </summary>
        [StringLength(512)]
        [Display(Name = "Couleur")]
        public  string? color { get; set; }
        /// <summary>
        /// The click action.
        /// </summary>
        [StringLength(512)]
        [Required, Display(Name = "Label du click")]
        public string click_action { get; set; }

        /// <summary>
        /// When null, this must be seen by everynone.
        /// <c>user/{UserId}<c> : it's for this user, and only this one, specified by ID,
        /// <c>pub/cga</c> : the public "cga" topic
        /// <c>administration</c> : for admins ...
        /// </summary>
        /// <returns></returns>
        [StringLength(512)]
        public string? Target { get; set; }

        public Notification()
        {
            icon = "exclam";
        }
    }
}
