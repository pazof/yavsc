using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Attributes.Validation;
using Yavsc.Interfaces;

namespace Yavsc.Models.Blog
{
    public class Comment : IComment<long>, IBaseTrackedEntity
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(1024)] 
        public string Content { get; set; }
        
        [ForeignKeyAttribute("PostId")][JsonIgnore]
        public virtual BlogPost Post { get; set; }

        [Required]
        public long PostId { get; set; }
        public bool Visible { get; set; }

        [ForeignKeyAttribute("AuthorId")][JsonIgnore]
        public virtual ApplicationUser Author {
            get; set;
        }

        [Required]
        public string AuthorId
        {
           get; set;
        }

        public string UserCreated
        {
           get; set;
        }
        public DateTime DateModified
        {
            get; set;
        }

        public string UserModified
        {
           get; set;
        }

        public DateTime DateCreated
        {
            get; set;
        }

        public long GetReceiverId()
        {
            return PostId;
        }        
        public void SetReceiverId(long rid)
        {
             PostId = rid;
        }

        public long? ParentId { get; set; }

        [ForeignKeyAttribute("ParentId")]
        public virtual Comment Parent { get; set; }

        [InversePropertyAttribute("Parent")]

        public virtual List<Comment> Children { get; set; }
    }
}
