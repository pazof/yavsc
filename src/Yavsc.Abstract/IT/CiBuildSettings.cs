using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Yavsc.Abstract.IT {

public class CiBuildSettings
{
   
    /// <summary>
    /// The global process environment variables
    /// </summary>
    /// <value></value>
    [JsonPropertyAttribute("env")]
    public string[] Environment { get; set; }

    /// <summary>
    /// The required building command.
    /// </summary>
    /// <value></value>
    [Required]
    [JsonPropertyAttribute("build")]
    public CommandPipe Build { get; set; }

    /// <summary>
    /// A preparing command.
    /// It is optional, but when specified, 
    /// must end ok in order to launch the build.
    /// </summary>
    /// <value></value>
    [JsonPropertyAttribute("prepare")]
    public CommandPipe Prepare { get; set; }

    /// <summary>
    /// A post-production command,
    /// for an example, some publishing,
    /// push in prod env ...
    /// only fired on successful build.
    /// </summary>
    /// <value></value>
    [JsonPropertyAttribute("post_build")]
    public CommandPipe PostBuild { get; set; }

    /// <summary>
    /// Additional emails, as dest of notifications
    /// </summary>
    /// <value></value>
    [JsonPropertyAttribute("emails")]
    public string[] Emails { get; set; }

}
}
