using System;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.Services;

public interface ILanCallSignaling
{
    event Action<CallSession>? Invitation;
    event Action<CallAccepted>? Accepted;
    event Action<CallEnded>? Ended;
    event Action<Guid, CallSignal>? Signal;
    event Action? Interrupted;
    Task<CallSession> InviteLanAsync(string room, string user, CancellationToken ct = default);
    Task AcceptAsync(Guid callId, CancellationToken ct = default);
    Task EndAsync(Guid callId, CancellationToken ct = default);
    Task SendAsync(Guid callId, CallSignal signal, CancellationToken ct = default);
}
