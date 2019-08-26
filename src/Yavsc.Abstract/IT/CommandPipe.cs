using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Yavsc.Abstract.IT
{
    public class CommandPipe
    {


        [JsonPropertyAttribute("pipe")]
        public Command[] Pipe { get; set; }


        [JsonPropertyAttribute("working_dir")]
        public string WorkingDir { get; set; }

        public virtual int Run()
        {
            Process latest = null;
            Queue<Process> runQueue = new Queue<Process>();
            Queue<Task> joints = new Queue<Task>();
            if (Pipe.Length == 0) return -1;
            if (Pipe.Length == 1)
            {
                latest = Pipe[0].Start();
                latest.WaitForExit();
                return latest.ExitCode;
            }

            for (int i = 0; i < Pipe.Length; i++)
            {
                Process newProcess = null;
                var cmd = Pipe[i];
                bool isNotLast = (i + 1) >= Pipe.Length;

                if (latest != null) // i.e. isNotFirst
                {
                    newProcess = cmd.Start(WorkingDir, true, isNotLast);
                    var jt = Task.Run(async () =>
                    {
                        while (!latest.HasExited && !newProcess.HasExited)
                        {
                            string line = await latest.StandardOutput.ReadLineAsync();
                            if (line != null)
                                await newProcess.StandardInput.WriteLineAsync(line);
                        }
                    });
                    joints.Enqueue(jt);
                }
                else
                {
                    newProcess = cmd.Start(WorkingDir, false, isNotLast);
                    Task ending = Task.Run(() => { latest.WaitForExit(); });
                    joints.Enqueue(ending);
                }
                latest = newProcess;
                runQueue.Enqueue(latest);
            }
            while (runQueue.Count > 0)
                (latest = runQueue.Dequeue()).WaitForExit();
            return latest.ExitCode;
        }
    }
}

