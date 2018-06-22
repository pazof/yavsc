using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Yavsc.Abstract.Interfaces;

namespace Yavsc.Server.Models.IT.SourceCode
{
    public abstract class GitBatch : Batch<GitRepositoryReference>
    {
        
        public GitBatch()
        {
            // git -c color.status=always status 
            // | ~/bin/ansi2html.sh --bg=dark --palette=xterm > ../test.html
           
        }

        ProcessStartInfo CreateAnsiFilter 
        (GitRepositoryReference input, params string [] args )
        {

            var pStart = new ProcessStartInfo("git", string.Join(" ", args));
            if (args[0]=="clone")
            pStart.WorkingDirectory = WorkingDir;
            else 
            pStart.WorkingDirectory = Path.Combine( WorkingDir, input.Path);
            return pStart;
        }

        protected ProcessStartInfo CreateProcessStart(string args)
        {
            return new ProcessStartInfo("git", args)
            { WorkingDirectory = WorkingDir };
        }
        bool Clone (GitRepositoryReference input)

        {
            var pStart = CreateProcessStart( $"clone -b {input.Branch} {input.Url} {input.Path}");
            pStart.WorkingDirectory = WorkingDir;
            var proc = Process.Start(pStart);
            proc.WaitForExit();
            return proc.ExitCode == 0;
        }
        bool Pull (GitRepositoryReference input)
        {
            LogPath = Path.Combine( WorkingDir, "git.log");
            var pStart = new ProcessStartInfo("git", "pull");

            pStart.WorkingDirectory = Path.Combine(WorkingDir,input.Path);
            pStart.RedirectStandardOutput = true;
            
            using (var mem = new MemoryStream())
            {
                using (var memWriter = new StreamWriter(mem))
                {
                var proc = Process.Start(pStart);
                    using (var memReader = new StreamReader(mem)) {
                        while (!proc.StandardOutput.EndOfStream)
                            memWriter.Write(proc.StandardOutput.Read());
                        proc.WaitForExit();
                        
                    }
                bool ok = proc.ExitCode==0;
                ResultHandler(ok);
                return ok;
                }
            }
        }
    }
}