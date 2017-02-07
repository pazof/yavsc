using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Access
{
  using YavscLib;

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
    }
}
