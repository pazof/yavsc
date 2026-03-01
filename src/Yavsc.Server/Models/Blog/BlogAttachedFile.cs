using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Org.BouncyCastle.Crypto.Modes;

namespace Yavsc.Models.Blog
{
  public class UploadedFile
  {
     /// <summary>
    /// File Identifier
    /// </summary>
    /// <value></value>
    [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Identifiant du fichier")]
    public long Id { get; set; }

    /// <summary>
    /// relative path to the file, relative to the poster space prefix
    /// </summary>
    /// <value></value>
    public string Path { get; set; }

    /// <summary>
    /// Length
    /// </summary> <summary>
    /// 
    /// </summary>
    /// <value></value>
    public long Length { get; set; }

    /// <summary>
    /// File Content type
    /// </summary>
    /// <value></value>
    public string ContentType { get; set; }
  }
  public class BlogAttachedFile
  {

    /// <summary>
    /// Post Id
    /// </summary>
    /// <value></value>
    public long FileId { get; set; }

    [ForeignKey("FileId")]
    public virtual UploadedFile File { get; set; }

    /// <summary>
    /// Post Id
    /// </summary>
    /// <value></value>
    public long PostId { get; set; }

    [ForeignKey("PostId")]
    public virtual BlogPost Post { get; set; }

  }
}
