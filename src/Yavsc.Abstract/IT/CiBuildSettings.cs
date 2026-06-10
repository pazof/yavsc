using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Yavsc.Abstract.IT {

public class CiBuildSettings
{
   
    /// <summary>
    /// The global process environment variables
    /// </summary>
    /// <value></value>
    [JsonPropertyName("env")]
    public string[] Environment { get; set; }

    /// <summary>
    /// The required building command.
    /// </summary>
    /// <value></value>
    [Required]
    [JsonPropertyName("build")]
    public CommandPipe Build { get; set; }

    /// <summary>
    /// A preparing command.
    /// It is optional, but when specified, 
    /// must end ok in order to launch the build.
    /// </summary>
    /// <value></value>
    [JsonPropertyName("prepare")]
    public CommandPipe Prepare { get; set; }

    /// <summary>
    /// A post-production command,
    /// for an example, some publishing,
    /// push in prod env ...
    /// only fired on successful build.
    /// </summary>
    /// <value></value>
    [JsonPropertyName("post_build")]
    public CommandPipe PostBuild { get; set; }

    /// <summary>
    /// Additional emails, as dest of notifications
    /// </summary>
    /// <value></value>
    [JsonPropertyName("emails")]
    public string[] Emails { get; set; }

}
}
