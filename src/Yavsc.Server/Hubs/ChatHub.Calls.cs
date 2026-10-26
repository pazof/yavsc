#nullable enable

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Chat.Calls;
using Yavsc.Server.Helpers;

namespace Yavsc.Server.Hubs;

public partial class ChatHub
{
    public Task<CallSession> InviteCall(string roomName, string callee) =>
        InviteCoreAsync(roomName, callee, CallTransports.WebRtc);

    public Task<CallSession> InviteLanCall(string roomName, string callee) =>
        InviteCoreAsync(roomName, callee, CallTransports.LanSrtp);

    private async Task<CallSession> InviteCoreAsync(string roomName, string callee, string transport)
    {
        var caller = CallUser();
        RequireSecureLanControl(transport);
        if (string.IsNullOrWhiteSpace(roomName) || !InputValidator.ValidateRoomName(roomName) ||
            string.IsNullOrWhiteSpace(callee) || !InputValidator.ValidateUserName(callee))
            throw new HubException("Invalid call participant or room.");
        RequireCallPresence(roomName, caller, callee);
        var relayOnly = await RequiresRelayAsync(caller, callee);
        return await _calls.InviteAsync(roomName, caller, callee, Context.ConnectionId,
            _cxManager.GetConnexionIds(callee)?.Distinct().ToArray() ?? [], relayOnly, transport);
    }

    public async Task AcceptCall(Guid callId)
    {
        var user = CallUser();
        var session = await _calls.GetSessionAsync(callId, user, Context.ConnectionId);
        RequireSecureLanControl(session.Transport);
        RequireCallPresence(session.RoomName, session.Caller, session.Callee);
        await _calls.AcceptAsync(callId, user, Context.ConnectionId,
            await RequiresRelayAsync(session.Caller, session.Callee));
    }

    public Task EndCall(Guid callId) => _calls.EndAsync(callId, CallUser(), Context.ConnectionId);

    public async Task SendCallSignal(Guid callId, CallSignal signal)
    {
        var user = CallUser();
        var session = await _calls.GetSessionAsync(callId, user, Context.ConnectionId);
        RequireSecureLanControl(session.Transport);
        RequireCallPresence(session.RoomName, session.Caller, session.Callee);
        if (!session.RelayOnly && await RequiresRelayAsync(session.Caller, session.Callee))
        {
            await _calls.EndForUserAsync(user, "privacy-changed");
            throw new HubException("Call ended because the IP privacy policy changed. Start a new call.");
        }
        await _calls.SignalAsync(callId, user, Context.ConnectionId, signal);
    }

    private string CallUser()
    {
        var user = Context.User?.GetUserName();
        if (string.IsNullOrWhiteSpace(user) || _cxManager.GetUserName(Context.ConnectionId) != user)
            throw new HubException("An authenticated chat connection is required.");
        return user;
    }

    private void RequireSecureLanControl(string transport)
    {
        if (transport == CallTransports.LanSrtp && Context.GetHttpContext()?.Request.IsHttps != true)
            throw new HubException("LAN calls require HTTPS signaling to protect SRTP keys.");
    }

    private void RequireCallPresence(string room, string caller, string callee)
    {
        if (!_cxManager.IsConnected(caller) || !_cxManager.IsConnected(callee) ||
            !_cxManager.IsPresent(room, caller) || !_cxManager.IsPresent(room, callee))
            throw new HubException("Both call participants must be present in the chat room.");
    }

    private async Task<bool> RequiresRelayAsync(string caller, string callee)
    {
        var users = await _dbContext.Users.AsNoTracking()
            .Where(u => u.UserName == caller || u.UserName == callee)
            .Select(u => new { u.Id, u.HideIpAddressInCalls }).ToArrayAsync(Context.ConnectionAborted);
        if (users.Length != 2)
            throw new HubException("Call participants are unavailable.");
        var ids = users.Select(u => u.Id).ToArray();
        if (await _dbContext.BlackListed.AnyAsync(
            b => ids.Contains(b.OwnerId) && ids.Contains(b.UserId), Context.ConnectionAborted))
            throw new HubException("Calls between these participants are not allowed.");
        return users.Any(u => u.HideIpAddressInCalls);
    }
}
