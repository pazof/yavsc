using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Abstract.Streaming;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Streaming
{

    public partial class LiveFlow : ILiveFlow {

      [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      [Display(Name="FlowIdLabel", ResourceType=typeof(LiveFlow))]
      // set by the server, unique
      public long Id { get; set; }

      //
      /// <summary>
      ///  a title for this flow
      /// </summary>
      /// <value></value>
      [YaStringLength(255)]
      [Display(Name="TitleLabel", ResourceType=typeof(LiveFlow))]
      public string Title { get; set; }

      // a little description
      [YaStringLength(1023)]
      [Display(Name="PitchLabel", ResourceType=typeof(LiveFlow))]
      public string Pitch { get; set; }

      // The stream type
      [YaStringLength(127)]
      [Display(Name="MediaTypeLabel", ResourceType=typeof(LiveFlow))]
      public string MediaType { get; set; }

      // A name where to save this stream, relative to user's files root
      [YaStringLength(255)]
      [Display(Name="DifferedFileNameLabel", ResourceType=typeof(LiveFlow))]
      public string DifferedFileName { get; set; }

      [Display(Name="SequenceNumberLabel", ResourceType=typeof(LiveFlow))]
      public int SequenceNumber { get; set; }

      [YaRequired]
      [Display(Name="OwnerIdLabel", ResourceType=typeof(LiveFlow))]
      public string OwnerId {get; set; }

      [ForeignKey("OwnerId")]
      [Display(Name="OwnerLabel", ResourceType=typeof(LiveFlow))]
      public virtual ApplicationUser Owner { get; set; }

    }
}     
