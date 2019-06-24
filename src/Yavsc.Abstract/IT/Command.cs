
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Newtonsoft.Json;
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
        /// Specific variables for this process
        /// </summary>
        /// <value></value>
        [JsonPropertyAttribute("env")]
        public string[] Environment { get; set; }

        public virtual Process Start(string workingDir=null, bool redirectInput=false, bool redirectOutput=false)
        {
            var procStart = new ProcessStartInfo(Path, string.Join(" ",Args));
            procStart.WorkingDirectory = workingDir;
            procStart.UseShellExecute = false;
            procStart.RedirectStandardInput = true;
            procStart.RedirectStandardOutput = true;
            procStart.RedirectStandardError = false;
            return Process.Start(procStart);
        }

    }
