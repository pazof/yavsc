using System;
using System.Diagnostics;
using System.IO;

namespace Yavsc.Server.Models.IT.SourceCode
{
    public class ProjectBuild : SingleCmdProjectBatch
    {
        public ProjectBuild(string repoRoot): base(repoRoot, "make")
        {
        }

        public override void Launch(Project input)
        {
            if (input==null) throw new ArgumentNullException("input");
            LogPath = $"{input.Name}.{_cmdPath}.ansi.log";
            
            // TODO honor Args property
            // Model annotations => input.Repository!=null => input.Name == input.Repository.Path
            var prjPath = Path.Combine(WorkingDir, input.Name);
            var repoInfo = new DirectoryInfo(prjPath);
            var makeStart = CreateStartInfo(prjPath);

            var args = string.Join(" ", Args);

            // TODO make `.ansi.log` a defined constant.
            var logFile = new FileInfo(Path.Combine(_repositoryRootPath, LogPath));
            using (var stream = logFile.Create())
            using (var writer = new StreamWriter(stream))
            {
                var process = Process.Start(makeStart);
                // TODO announce ... 
                while (!process.HasExited)
                {
                    if (process.StandardOutput.Peek() > -1)
                        writer.WriteLine(process.StandardOutput.ReadLine());
                }
            }
            ResultHandler?.Invoke(true);
        }
        
    }
}
