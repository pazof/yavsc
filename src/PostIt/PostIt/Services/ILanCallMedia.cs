using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.Services;

public interface ILanCallMedia : IAsyncDisposable
{
    Control LocalVideo { get; }
    Control RemoteVideo { get; }
    LanCallEndpoint Endpoint { get; }
    Task Completion { get; }
    bool HasRemoteMedia { get; }
    Task StartSendingAsync(LanCallEndpoint remote, string camera, string microphone, CancellationToken ct);
}
