using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Abstract.Streaming;
using Yavsc.Models;

namespace Yavsc.Models.Streaming
{

    public class LiveFlow : ILiveFlow {

      [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      [Display(Name="FlowId")]
      // set by the server, unique
      public long Id { get; set; }

      // a title for this flow
      public string Title { get; set; }

      // a little description
      public string Pitch { get; set; }

      // The stream type
      public string MediaType { get; set; }

      // A name where to save this stream, relative to user's files root
      public string DifferedFileName { get; set; }

      [Required]
      public string OwnerId {get; set; }

      [ForeignKey("OwnerId")]
      public virtual ApplicationUser Owner { get; set; }

    }
}     
