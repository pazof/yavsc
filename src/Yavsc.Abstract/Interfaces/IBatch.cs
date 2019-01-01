using System;

namespace Yavsc.Abstract.Interfaces
{
    public interface IBatch<TInput, TOutput>
    {
        string WorkingDir { get; set; }

        Action<TOutput> ResultHandler { get; }
        void Launch(TInput Input);
        string LogPath { get; set; }
    }
}