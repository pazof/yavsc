
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace YaDaemon
{
    public class YaTaskScheduler : TaskScheduler
    {
        List<Task> _tasks;

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _tasks;
        }

        protected override void QueueTask(Task task)
        {
            _tasks.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            task.Start();
            task.Wait();
            return task.IsCompleted;
        }
    }
    public class YaDaemon: IDisposable
    {
        
        private readonly EventLog _log =
            new EventLog("Application") { Source = "Application" };

        async void MainLoop(string[] args)
        {

        }
        public async static void Main(string[] args)
        {
             using (var prog = new YaDaemon()) {
                 try {
                    await prog.StartAsync(args);
                 
                 } catch (Exception ex)
                 {
                    prog.OnContinue();
                 }
                 finally {
                    prog.OnShutdown();
                 }

                
             }
             
        }
        async Task StartAsync(string[] args)
        {
            await Task.Run(() => {
                OnStart(args);
            } );
        }

        protected void OnContinue()
        {
            
        }


        protected void OnShutdown()
        {
            
        }



        protected void OnStart(string[] args)
        {
            _log.WriteEntry("Test from YaDaemon.", EventLogEntryType.Information, 1);
            _log.WriteEntry("YaDaemon started.");
            Console.WriteLine("YaDaemon started");
        }

        protected  void OnStop()
        {
            _log.WriteEntry("YaDaemon stopped.");
            Console.WriteLine("YaDaemon stopped");
        }

        public void Dispose()
        {
        }
    }
}