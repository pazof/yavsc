using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Access
{
    public class BlackListed
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string UserId { get; set; }
        public long ListId { get; set; }

        [ForeignKey("ListId")]
        public virtual BlackList BlackList { get; set; }
    }
    public class BlackList
    {
        public BlackList(long id, string target)
        {
            this.Id = id;
            this.Target = target;

        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Target { get; set; }

        [InverseProperty("BlackList")]
        public virtual List<BlackListed> Items
         {
             get; set;
         }
    }
}
