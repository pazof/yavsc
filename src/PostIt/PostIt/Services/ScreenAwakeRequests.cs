using System;
using System.Threading;

namespace PostIt.Services;

public sealed class ScreenAwakeRequests
{
    private int _count;
    public bool IsRequested => Volatile.Read(ref _count) > 0;
    public event Action? Changed;

    public IDisposable Acquire()
    {
        Interlocked.Increment(ref _count);
        Changed?.Invoke();
        return new Request(this);
    }

    private sealed class Request(ScreenAwakeRequests owner) : IDisposable
    {
        private ScreenAwakeRequests? _owner = owner;

        public void Dispose()
        {
            var current = Interlocked.Exchange(ref _owner, null);
            if (current is null) return;
            Interlocked.Decrement(ref current._count);
            current.Changed?.Invoke();
        }
    }
}
