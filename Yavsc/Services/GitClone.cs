// // GitClone.cs
// /*
// paul  21/06/2018 11:27 20182018 6 21
// */
using Yavsc.Server.Models.IT.SourceCode;
using Yavsc.Server.Models.IT;
using System.Diagnostics;
using System.IO;

namespace Yavsc.Lib
{
    public class GitClone : Batch<Project>
    {
        string _repositoryRootPath;

        public GitClone(string repoRoot)
        {
            _repositoryRootPath = repoRoot;
        }

        public override void Launch(Project input)
        {
            WorkingDir = _repositoryRootPath;
            LogPath = $"{input.Name}.git-clone.ansi.log";
            // TODO honor Args property
            // Model annotations => input.Repository!=null => input.Name == input.Repository.Path
            var prjPath = Path.Combine(WorkingDir, input.Name);
            var repoInfo = new DirectoryInfo(prjPath);
            var gitCmd = repoInfo.Exists ? "pull" : "clone";

            var cloneStart = new ProcessStartInfo
            ( gitCmd, $"{gitCmd} {input.Repository.Url} {input.Repository.Path}" )
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
                // TODO announce ... 
                while (!process.HasExited)
                {
                    if (process.StandardOutput.Peek() > -1)
                        writer.WriteLine(process.StandardOutput.ReadLine());
                }
            }
            ResultHandler(true);
        }
    }
}
