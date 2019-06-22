using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Abstract.Streaming;

namespace Yavsc.Models.Streaming
{

    public partial class LiveFlow : ILiveFlow {

      [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      [Display(Name="FlowIdLabel")]
      // set by the server, unique
      public long Id { get; set; }

      //
      /// <summary>
      ///  a title for this flow
      /// </summary>
      /// <value></value>
      [StringLength(255)]
      [Display(Name="TitleLabel")]
      public string Title { get; set; }

      // a little description
      [StringLength(1023)]
      [Display(Name="PitchLabel")]
      public string Pitch { get; set; }

      // The stream type
      [StringLength(127)]
      [Display(Name="MediaTypeLabel")]
      public string MediaType { get; set; }

      // A name where to save this stream, relative to user's files root
      [StringLength(255)]
      [Display(Name="DifferedFileNameLabel")]
      public string DifferedFileName { get; set; }
      public int SequenceNumber { get; set; }

      [Required]
      [Display(Name="OwnerIdLabel")]
      public string OwnerId {get; set; }

      [ForeignKey("OwnerId")]
      [Display(Name="OwnerLabel")]
      public virtual ApplicationUser Owner { get; set; }

    }
}     
