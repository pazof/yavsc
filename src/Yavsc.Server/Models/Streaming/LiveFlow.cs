using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models;

namespace Yavsc.Models.Streaming
{

    public class LiveFlow {

      [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      [Display(Name="FlowId")]
      // set by the server, unique
      long Id { get; set; }

      // a title for this flow
      string Title { get; set; }

      // a little description
      string Pitch { get; set; }

      // The stream type
      string MediaType { get; set; }

      // A name where to save this stream, relative to user's files root
      string DifferedFileName { get; set; }

      [Required]
      public string OwnerId {get; set; }

      [ForeignKey("OwnerId")]
      public virtual ApplicationUser Owner { get; set; }

    }
}     
