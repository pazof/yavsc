
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{

 public class Command {

    /// <summary>
    /// The command identifier
    /// </summary>
    [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id {get; set; }

    public string ClientId { get; set; }

    /// <summary>
    /// The client
    /// </summary>
    [Required,ForeignKey("ClientId")]
    public ApplicationUser Client { get; set; }

    [Required]
    public string PerformerId { get; set; }
    /// <summary>
    /// The performer identifier
    /// </summary>
    [ForeignKey("PerformerId")]
    public PerformerProfile PerformerProfile { get; set; }

    public DateTime? ValidationDate {get; set;}

    /// <summary>
    /// Command creation Date & time
    /// </summary>
    /// <returns></returns>
    public DateTime CreationDate {get; set;}

    public decimal? Previsional { get; set; }
    public List<CommandLine> Bill { get; set; }

    ///<summary>
    /// Time span in seconds in which
    /// Validation will be considered as definitive
    ///</summary>
    public int Lag { get; set; }
 }
}