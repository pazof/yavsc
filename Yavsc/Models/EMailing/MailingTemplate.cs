using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models;
using Yavsc.Models.Calendar;

namespace Yavsc.Server.Models.EMailing
{
    public class MailingTemplate : IBaseTrackedEntity
    {
        /// <summary>
        /// Date Created
        /// </summary>
        /// <returns></returns>
        public DateTime DateCreated
        {
            get;
            set;
        }

        public DateTime DateModified
        {
            get;
            set;
        }

        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [MaxLengthAttribute(128)]
        public string ShortName { get; set; }

        /// <summary>
        /// Markdown template to process
        /// </summary>
        /// <returns></returns>
        [MaxLengthAttribute(64*1024)]
        public string Body { get; set; }

        [EmailAddress()]
        public string ReplyToAddress { get; set; }

        [ForeignKey("ManagerId")]
        public virtual ApplicationUser Manager { get; set; }

        public Periodicity ToSend { get; set; }
        public string ManagerId
        {
            get;
            set;
        }

        public string UserCreated
        {
            get;
            set;
        }

        public string UserModified
        {
            get;
            set;
        }
    }
}
