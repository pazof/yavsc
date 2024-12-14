using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;
using Yavsc.Models;
using Yavsc.Models.Calendar;
using Yavsc.Server.Models.Calendar;

namespace Yavsc.Server.Models.EMailing
{
    public class MailingTemplate : ITrackedEntity
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

        [Key][YaStringLength(3, 256)]
        public string Id { get; set; }

        [YaStringLength(3, 256)]
        public string Topic { get; set; }

        /// <summary>
        /// Markdown template to process
        /// </summary>
        /// <returns></returns>
        [MaxLength(64*1024)]
        public string Body { get; set; }

        [EmailAddress()]
        public string ReplyToAddress { get; set; }


        public Periodicity ToSend { get; set; }
        
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
