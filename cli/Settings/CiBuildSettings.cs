using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

public class CiBuildSettings
{
    /// <summary>
    /// A command specification (a system command),
    /// in order to reference some trusted server-side process
    /// </summary>
    public class Command
    {
        [Required]
        [JsonPropertyAttribute("path")]
        public string Path { get; set; }

        [JsonPropertyAttribute("args")]
        public string[] Args { get; set; }

        /// <summary>
        /// Specific variables to this process
        /// </summary>
        /// <value></value>
        /// 
        [JsonPropertyAttribute("env")]
        public string[] Environment { get; set; }
        
        [JsonPropertyAttribute("working_dir")]
        public string WorkingDir { get; set; }
    }
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
    public Command Build { get; set; }

    /// <summary>
    /// A preparing command.
    /// It is optional, but when specified, 
    /// must end ok in order to launch the build.
    /// </summary>
    /// <value></value>
    [JsonPropertyAttribute("prepare")]
    public Command Prepare { get; set; }

    /// <summary>
    /// A post-production command,
    /// for an example, some publishing,
    /// push in prod env ...
    /// only fired on successful build.
    /// </summary>
    /// <value></value>
    [JsonPropertyAttribute("post_build")]
    public Command PostBuild { get; set; }

    /// <summary>
    /// Additional emails, as dest of notifications
    /// </summary>
    /// <value></value>
    [JsonPropertyAttribute("emails")]
    public string[] Emails { get; set; }

}