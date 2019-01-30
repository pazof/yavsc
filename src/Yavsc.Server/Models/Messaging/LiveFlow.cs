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

      //
      /// <summary>
      ///  a title for this flow
      /// </summary>
      /// <value></value>
      [StringLength(255)]
      public string Title { get; set; }

      // a little description
      [StringLength(1023)]
      public string Pitch { get; set; }

      // The stream type
      [StringLength(127)]
      public string MediaType { get; set; }

      // A name where to save this stream, relative to user's files root
      [StringLength(255)]
      public string DifferedFileName { get; set; }
      public int SequenceNumber { get; set; }

      [Required]
      public string OwnerId {get; set; }

      [ForeignKey("OwnerId")]
      public virtual ApplicationUser Owner { get; set; }

    }
}     
