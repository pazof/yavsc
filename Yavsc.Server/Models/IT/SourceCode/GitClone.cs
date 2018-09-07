// // GitClone.cs
// /*
// paul  21/06/2018 11:27 20182018 6 21
// */
using System.Diagnostics;
using System.IO;
using System;

namespace Yavsc.Server.Models.IT.SourceCode
{
    public class GitClone : SingleCmdProjectBatch
    {

        public GitClone(string repo) : base(repo,"git")
        {

        }
        public override void Launch(Project input)
        {
            if (input==null) throw new ArgumentNullException("input");
            LogPath = $"{input.Name}.git-clone.ansi.log";
            // TODO honor Args property
            // Model annotations => input.Repository!=null => input.Name == input.Repository.Path
            var prjPath = Path.Combine(WorkingDir, input.Name);
            var repoInfo = new DirectoryInfo(prjPath);
            var gitCmd = repoInfo.Exists ? "pull" : "clone --depth=1";

            var cloneStart = new ProcessStartInfo
            ( _cmdPath, $"{gitCmd} {input.Repository.Url} {input.Repository.Path}" )
            {
                WorkingDirectory = WorkingDir,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            // TODO make `.ansi.log` a defined constant.
            var logFile = new FileInfo
                ( Path.Combine
                 ( _repositoryRootPath, $"{input.Name}.ansi.log" ));
            using (var stream = logFile.Create())
            using (var writer = new StreamWriter(stream))
            {
                var process = Process.Start(cloneStart);
                // TODO publish the starting log url ... 
                while (!process.HasExited)
                {
                    if (process.StandardOutput.Peek() > -1)
                        writer.WriteLine(process.StandardOutput.ReadLine());
                }
            }
            if (ResultHandler!=null) ResultHandler(true);
        }
    }
}
