using System;
using Yavsc.Abstract.Interfaces;

namespace Yavsc.Server.Models.IT.SourceCode
{
    public abstract class Batch<TInput> : IBatch<TInput, bool>
    {
        public string WorkingDir { get; set; }
        public string LogPath { get; set; }
        public Action<bool> ResultHandler { get; private set; }
        public string []Args { get; set; }
        public abstract void Launch(TInput Input);
    }
}