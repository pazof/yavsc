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
            Queue<Process> runQueue = new Queue<Process>();
            Queue<Task> joints = new Queue<Task>();
            if (Pipe.Length == 0) return -1;
            if (Pipe.Length == 1)
            {
                Process singlecmd = Pipe[0].Start();
                singlecmd.WaitForExit();
                return singlecmd.ExitCode;
            }

            Command cmd = Pipe[0];
            Process newProcess = cmd.Start(WorkingDir, false, true);
            Process latest = newProcess;
            Task ending = Task.Run(() => { latest.WaitForExit(); });
            for (int i = 1; i < Pipe.Length; i++)
            {
                joints.Enqueue(ending);
                cmd = Pipe[i];
                bool isNotLast = i < Pipe.Length;

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
               
                latest = newProcess;
                runQueue.Enqueue(latest);
            }
            while (runQueue.Count > 0)
                (latest = runQueue.Dequeue()).WaitForExit();
            return latest.ExitCode;
        }
    }
}

