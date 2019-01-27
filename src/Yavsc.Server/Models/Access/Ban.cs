using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Access
{
    using Yavsc;
    public class Ban : IBaseTrackedEntity
    {
        public DateTime DateCreated
        {
            get; set;
        }

        public DateTime DateModified
        {
            get; set;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string UserCreated
        {
            get; set;
        }

        public string UserModified
        {
            get; set;
        }

        [Required]
        public string TargetId
        {
            get; set;
        }

        [ForeignKey("TargetId")]
        public virtual ApplicationUser TargetUser {
            get; set;
        }

        [Required]
        public string Reason { get; set; }

    }
}
