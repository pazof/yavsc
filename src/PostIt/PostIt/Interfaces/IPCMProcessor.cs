using System;

public interface IPCMProcessor : IDisposable
{
    void Process(short[] audioFrame);
}
