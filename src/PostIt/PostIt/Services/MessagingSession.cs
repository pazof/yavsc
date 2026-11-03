using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using PostIt.ViewModels;
using PostIt.ViewModels.Chat;

namespace PostIt.Services;

/// <summary>Coordinates process-scoped chat and the optional native background service.</summary>
public sealed class MessagingSession : IAsyncDisposable
{
    private readonly YavscApiClient _api;
    private readonly SessionStatusViewModel _session;
    private readonly ChatViewModel _chat;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly SemaphoreSlim _gate = new(1);
    private bool _started;
    private bool _connectOnLogin;
    private bool? _servicePreference;
    private bool _signingOut;
    private Task? _maintenance;
    private string? _apiUrl;
    private string? _authority;
    private CancellationTokenSource? _serviceOperation;
    private readonly object _operationGate = new();

    public MessagingSession(YavscApiClient api, SessionStatusViewModel session, ChatViewModel chat)
    {
        _api = api;
        _session = session;
        _chat = chat;
    }

    public async Task StartAsync(bool connectOnLogin = true)
    {
        if (_started) return;
        _started = true;
        _connectOnLogin = connectOnLogin;
        _apiUrl = _api.Settings.ApiUrl;
        _authority = _api.Settings.Authentication.Authority;
        _api.SigningOut += OnSigningOutAsync;
        _session.PropertyChanged += OnSessionChanged;
        _api.Settings.Saved += OnSettingsSaved;
        _chat.ConnectionRequestedChanged += OnConnectionRequestedChanged;
        if (_api.HasValidSession && _connectOnLogin) await _chat.ConnectAsync();
        _maintenance = MaintainChatAsync(_lifetime.Token);
        await SynchronizeServiceAsync();
    }

    private async Task MaintainChatAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (!_api.HasValidSession && _chat.ConnectionRequested)
                {
                    try { await OnSigningOutAsync(); }
                    catch (Exception ex)
                    {
                        _api.Settings.SetActionStatus($"Arret du service de messagerie incomplet : {ex.Message}", Models.StatusSeverity.Error);
                    }
                    _session.Refresh();
                }
                if (!_signingOut && _api.HasValidSession && !_chat.IsConnected && _chat.ConnectionRequested)
                    await _chat.ConnectAsync();
                await SynchronizeServiceAsync();
                await Task.Delay(TimeSpan.FromSeconds(30), ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
    }

    private async void OnSessionChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(SessionStatusViewModel.IsLoggedIn)) return;
        if (_session.IsLoggedIn) _signingOut = false;
        await SynchronizeServiceAsync();
        if (_session.IsLoggedIn && (_connectOnLogin || _chat.ConnectionRequested))
        {
            await _chat.ConnectAsync();
        }

    }

    private async void OnConnectionRequestedChanged()
    {
        if (_signingOut || _lifetime.IsCancellationRequested) return;
        await SynchronizeServiceAsync();
    }

    private async void OnSettingsSaved(object? sender, EventArgs args)
    {
        try
        {
            if (!_api.Settings.AndroidKeepChatConnected) CancelServiceOperation();
            if (_apiUrl != _api.Settings.ApiUrl || _authority != _api.Settings.Authentication.Authority)
            {
                _apiUrl = _api.Settings.ApiUrl;
                _authority = _api.Settings.Authentication.Authority;
                _servicePreference = null;
                var reconnect = _connectOnLogin || _chat.ConnectionRequested;
                await _chat.DisconnectAsync();
                if (_api.HasValidSession && reconnect) await _chat.ConnectAsync();
            }
            await SynchronizeServiceAsync();
        }
        catch (Exception ex)
        {
            _api.Settings.SetActionStatus($"Mise a jour de la messagerie impossible : {ex.Message}", Models.StatusSeverity.Error);
        }
    }

    private async Task SynchronizeServiceAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var native = Platform.BackgroundChatService;
            if (native == null || !_api.HasValidSession || _api.IsSigningOut || _signingOut) return;
            var enabled = _api.Settings.AndroidKeepChatConnected && _chat.ConnectionRequested;
            if (_servicePreference == enabled) return;
            try
            {
                using var serviceOperation = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
                lock (_operationGate) _serviceOperation = serviceOperation;
                try
                {
                    await native.SynchronizeAsync(_api, enabled, serviceOperation.Token);
                    _servicePreference = enabled;
                }
                finally { lock (_operationGate) _serviceOperation = null; }
            }
            catch (OperationCanceledException) when (_signingOut || _lifetime.IsCancellationRequested || !_api.Settings.AndroidKeepChatConnected) { }
            catch (Exception ex)
            {
                _api.Settings.SetActionStatus($"Service de messagerie Android indisponible : {ex.Message}", Models.StatusSeverity.Error);
            }
        }
        finally { _gate.Release(); }
    }

    private async Task OnSigningOutAsync()
    {
        _signingOut = true;
        CancelServiceOperation();
        await _gate.WaitAsync();
        try
        {
            _servicePreference = null;
            try
            {
                if (Platform.BackgroundChatService is { } native)
                    await native.DisableAsync(_api);
            }
            finally { await _chat.DisconnectAsync(); }
        }
        finally { _gate.Release(); }
    }

    public async ValueTask DisposeAsync()
    {
        _api.SigningOut -= OnSigningOutAsync;
        _session.PropertyChanged -= OnSessionChanged;
        _api.Settings.Saved -= OnSettingsSaved;
        _chat.ConnectionRequestedChanged -= OnConnectionRequestedChanged;
        _lifetime.Cancel();
        if (_maintenance != null) await _maintenance;
        await _chat.DisconnectAsync();
        _lifetime.Dispose();
    }

    private void CancelServiceOperation()
    {
        lock (_operationGate) _serviceOperation?.Cancel();
    }
}
