using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Models;
using PostIt.Services;
using Yavsc;

namespace PostIt.ViewModels.Chat
{
    /// <summary>
    /// ViewModel pour la gestion du chat,
    /// incluant la connexion au Hub SignalR,
    /// la gestion des salons et des utilisateurs connectés.
    /// </summary>

    public partial class ChatViewModel : ViewModelBase
    {
        private readonly Settings _settings;
        private readonly YavscApiClient? _api;
        private readonly ChatHubConnection? _transport;
        private bool _handlersRegistered;
        private readonly SemaphoreSlim _connectionGate = new(1);
        private int _sessionVersion;
        private readonly object _connectOperationGate = new();
        private CancellationTokenSource? _connectOperation;
        private long _lastDeliverySequence;
        public bool ConnectionRequested { get; private set; }
        public event Action? ConnectionRequestedChanged;

        // Le paramètre optionnel permet l'instanciation design-time
        // (Design.DataContext dans ChatPage.axaml) sans conteneur DI.
        public ChatViewModel(YavscApiClient? api = null, Settings? settings = null, ChatHubConnection? transport = null)
        {
            var app = App.Current as App;
            _settings = settings ?? app!.ServiceProvider!.GetRequiredService<Settings>();
            _api = api;
            _transport = transport;
            if (_transport != null)
            {
                _transport.RoomJoined += room => PostForCurrentSession(() => OnRoomJoined(room));
                _transport.Error += message => PostForCurrentSession(() => Log(message));
                _transport.MessageReceived += delivery => PostForCurrentSession(() => OnMessageReceived(delivery));
            }
            Status = new StatusNotice("Déconnecté", StatusSeverity.Info,
            new RelayCheckCommand("Connecter", async () => await ConnectAsync()));
            FocusedMessages = _systemMessages;
        }

        private HubConnection? _connection;

        public CallSignalingClient? Calls { get; private set; }

        public bool SupportsVideoCalls => Platform.CreateWebRtcMedia != null ||
            (OperatingSystem.IsLinux() && !OperatingSystem.IsAndroid());

        public bool SupportsLanCalls => Platform.CreateLanCallMedia != null ||
            (OperatingSystem.IsLinux() && !OperatingSystem.IsAndroid());

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenCallCommand), nameof(ShowCallCommand))]
        public partial WebRtcCallViewModel? VideoCall { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenCallCommand), nameof(ShowCallCommand))]
        public partial LanCallViewModel? LanCall { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenCallCommand))]
        public partial string? SelectedUser { get; set; }

        [ObservableProperty]
        public partial string CurrentRoom { get; set; } = "General";

        [ObservableProperty]
        public partial string NickName { get; set; } = "";

        [ObservableProperty]
        public partial StatusNotice Status { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        [NotifyCanExecuteChangedFor(nameof(OpenCallCommand), nameof(ShowCallCommand))]
        public partial bool IsConnected { get; set; } = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        public partial string MessageToSend { get; set; } = "";

        // Salles jointes (choix de la salle en focus dans la vue).
        public ObservableCollection<string> JoinedRooms { get; } = new();

        // Salle en focus ; null = aucune salle, le journal affiche
        // alors les notifications système.
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        [NotifyCanExecuteChangedFor(nameof(OpenCallCommand))]
        public partial string? SelectedRoom { get; set; }

        // Notifications globales (connexion, erreurs de join, ...)
        private readonly ObservableCollection<string> _systemMessages = new();

        // Messages et utilisateurs présents, indexés par salle.
        private readonly Dictionary<string, ObservableCollection<string>> _messagesByRoom = new();
        private readonly Dictionary<string, ObservableCollection<string>> _usersByRoom = new();
        private static readonly ObservableCollection<string> _noUsers = new();

        // Vues "focus" exposées à la page : pointent vers la salle
        // sélectionnée, ou vers le journal système / une liste vide.
        [ObservableProperty]
        public partial ObservableCollection<string> FocusedMessages { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<string> FocusedUsers { get; set; } = _noUsers;

        partial void OnFocusedUsersChanging(ObservableCollection<string> value) =>
            FocusedUsers.CollectionChanged -= OnFocusedUsersCollectionChanged;

        partial void OnFocusedUsersChanged(ObservableCollection<string> value)
        {
            value.CollectionChanged += OnFocusedUsersCollectionChanged;
            OpenCallCommand.NotifyCanExecuteChanged();
        }

        private void OnFocusedUsersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (SelectedUser != null && !FocusedUsers.Contains(SelectedUser)) SelectedUser = null;
            OpenCallCommand.NotifyCanExecuteChanged();
        }

        private void UpdateRoomUsers(string type, string room, string user)
        {
            if (!_usersByRoom.TryGetValue(room, out var users)) return;
            if (string.Equals(type, NotificationTypes.UserJoin, StringComparison.OrdinalIgnoreCase))
            {
                if (!users.Contains(user)) users.Add(user);
            }
            else if (string.Equals(type, NotificationTypes.UserPart, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(type, NotificationTypes.Kick, StringComparison.OrdinalIgnoreCase))
            {
                var separator = user.IndexOf(':');
                users.Remove(separator < 0 ? user : user[..separator]);
            }
        }

        partial void OnSelectedRoomChanged(string? value)
        {
            SelectedUser = null;
            if (VideoCall != null) VideoCall.Room = value ?? "";
            if (LanCall != null) LanCall.Room = value ?? "";
            FocusedMessages = value != null && _messagesByRoom.TryGetValue(value, out var msgs)
                ? msgs : _systemMessages;
            FocusedUsers = value != null && _usersByRoom.TryGetValue(value, out var users)
                ? users : _noUsers;
        }

        private bool CanShowCall() => IsConnected && (VideoCall != null || LanCall != null);

        private bool CanOpenCall() => CanShowCall() && SelectedRoom != null &&
            !string.IsNullOrWhiteSpace(SelectedUser) && FocusedUsers.Contains(SelectedUser);

        internal CallPageViewModel PrepareCallPage()
        {
            if (!CanOpenCall())
                throw new InvalidOperationException("Selectionnez un utilisateur present dans le salon et connectez le chat.");
            if (VideoCall?.Busy != true && LanCall?.Busy != true)
            {
                if (VideoCall != null) VideoCall.Target = SelectedUser!;
                if (LanCall != null) LanCall.Target = SelectedUser!;
            }
            return new CallPageViewModel(this);
        }

        [RelayCommand(CanExecute = nameof(CanOpenCall))]
        private Task OpenCallAsync() => NavigateToCallAsync(configureTarget: true);

        [RelayCommand(CanExecute = nameof(CanShowCall))]
        private Task ShowCallAsync() => NavigateToCallAsync(configureTarget: false);

        private async Task NavigateToCallAsync(bool configureTarget)
        {
            try
            {
                var app = App.Current as App ?? throw new InvalidOperationException("Application indisponible.");
                var page = configureTarget ? PrepareCallPage() : new CallPageViewModel(this);
                await app.PushPageAsync(page);
            }
            catch (Exception error) { Log($"Impossible d'ouvrir l'appel : {error.Message}"); }
        }

        public async Task HangupCallsAsync()
        {
            if (VideoCall != null) await VideoCall.HangupAsync();
            if (LanCall != null) await LanCall.HangupAsync();
        }

        // Trace une notification système dans la vue courante
        // (et dans le journal global pour consultation ultérieure).
        private void Log(string line)
        {
            _systemMessages.Add(line);
            if (!ReferenceEquals(FocusedMessages, _systemMessages))
                FocusedMessages.Add(line);
        }

        private void PostForCurrentSession(Action callback)
        {
            var version = Volatile.Read(ref _sessionVersion);
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (ConnectionRequested && version == Volatile.Read(ref _sessionVersion))
                    callback();
            });
        }

        private void OnMessageReceived(ChatHubConnection.ChatDelivery delivery)
        {
            if (delivery.Sequence <= _lastDeliverySequence) return;
            _lastDeliverySequence = delivery.Sequence;
            if (delivery.Room == null)
            {
                Log($"[Prive] {delivery.Sender} : {delivery.Text}");
                return;
            }
            if (!_messagesByRoom.TryGetValue(delivery.Room, out var messages))
                messages = _messagesByRoom[delivery.Room] = new();
            messages.Add($"{delivery.Sender} : {delivery.Text}");
        }

        // Commandes pour les boutons
        [RelayCommand]
        public async Task ConnectCommand() => await ConnectAsync();

        [RelayCommand]
        public async Task JoinRoomCommand() => await JoinRoomAsync();

        [RelayCommand]
        public async Task LeaveRoom()
        {
            var room = SelectedRoom;
            if (_transport == null || room == null) return;
            try
            {
                await HangupCallsAsync();
                await _transport.LeaveAsync(room);
                JoinedRooms.Remove(room);
                _messagesByRoom.Remove(room);
                _usersByRoom.Remove(room);
                SelectedRoom = JoinedRooms.Count > 0 ? JoinedRooms[0] : null;
                Log($"Salon quitte : {room}");
            }
            catch (Exception ex) { Log($"Impossible de quitter le salon : {ex.Message}"); }
        }

        // Le serveur (ChatHub.Send) refuse tout message d'un utilisateur
        // qui n'a pas joint le salon ("NoJoinNoSend") : on garde la
        // commande inactive tant qu'aucune salle n'est en focus.
        private bool CanSendMessage() =>
            IsConnected && SelectedRoom != null && !string.IsNullOrWhiteSpace(MessageToSend);

        [RelayCommand(CanExecute = nameof(CanSendMessage))]
        private async Task SendMessageAsync()
        {
            if (_connection == null || SelectedRoom == null) return;
            var text = MessageToSend.Trim();
            try
            {
                await _connection.InvokeAsync("Send", SelectedRoom, text);
                MessageToSend = string.Empty;
            }
            catch (Exception ex)
            {
                Log($"Erreur d'envoi : {ex.Message}");
            }
        }

        public override bool CanNavigateNext { get; protected set; } = false;
        public override bool CanNavigatePrevious { get; protected set; } = true;

        public async Task DisconnectAsync()
        {
            ConnectionRequested = false;
            ConnectionRequestedChanged?.Invoke();
            lock (_connectOperationGate) _connectOperation?.Cancel();
            Interlocked.Increment(ref _sessionVersion);
            await _connectionGate.WaitAsync();
            try { await DisconnectCoreAsync(); }
            finally { _connectionGate.Release(); }
        }

        private async Task DisconnectCoreAsync()
        {
            Exception? callError = null;
            try { await DisposeCallViewModelsAsync(); }
            catch (Exception error) { callError = error; }
            Calls?.Dispose();
            Calls = null;
            try
            {
                if (_connection != null)
                    await _transport!.StopAsync();
            }
            catch (Exception error) { callError = error; throw; }
            finally
            {
                IsConnected = false;
                SelectedRoom = null;
                JoinedRooms.Clear();
                _messagesByRoom.Clear();
                _usersByRoom.Clear();
                _systemMessages.Clear();
                Status = new StatusNotice(callError == null ? "Déconnecté" :
                    "Déconnecté ; erreur de fermeture : " + callError.Message,
                    callError == null ? StatusSeverity.Info : StatusSeverity.Error,
                 new RelayCheckCommand("Connecter", async () => await ConnectAsync()));
            }
        }

        private async Task DisposeCallViewModelsAsync()
        {
            Exception? callError = null;
            if (VideoCall != null)
            {
                try { await VideoCall.DisposeAsync(); }
                catch (Exception error) { callError = error; }
                finally { VideoCall = null; }
            }
            if (LanCall != null)
            {
                try { await LanCall.DisposeAsync(); }
                catch (Exception error) { callError = callError == null ? error : new AggregateException(callError, error); }
                finally { LanCall = null; }
            }
            if (callError != null) throw callError;
        }

        public async Task ConnectAsync()
        {
            if (_api?.IsSigningOut == true)
            {
                Status = StatusNotice.Info("La deconnexion du compte est en cours.");
                return;
            }
            ConnectionRequested = true;
            ConnectionRequestedChanged?.Invoke();
            await _connectionGate.WaitAsync();
            try
            {
                if (!ConnectionRequested) return;
                using var operation = new CancellationTokenSource();
                lock (_connectOperationGate) _connectOperation = operation;
                try { await ConnectCoreAsync(operation.Token); }
                finally { lock (_connectOperationGate) _connectOperation = null; }
            }
            finally { _connectionGate.Release(); }
        }

        private async Task ConnectCoreAsync(CancellationToken ct)
        {
            try
            {
                if (_transport?.IsCurrentConfiguration == true &&
                    _connection?.State is HubConnectionState.Connected or HubConnectionState.Reconnecting)
                    return;
                if (_connection != null)
                {
                    try { await DisposeCallViewModelsAsync(); }
                    finally
                    {
                        Calls?.Dispose();
                        Calls = null;
                    }
                }
                Status = new StatusNotice("Connexion en cours...", StatusSeverity.Info,
                 new RelayCheckCommand("Vérifier la connexion", async () => await CheckConnectionAsync()));

                if (_api == null || !_api.HasValidSession)
                {
                    Status = new StatusNotice("Session expirée : reconnectez-vous avant d'ouvrir le chat.", StatusSeverity.Error,
                    new RelayCheckCommand("Reconnecter", async () => await ConnectAsync()));
                    return;
                }

                if (_transport == null)
                    throw new InvalidOperationException("Transport de messagerie indisponible.");
                var connection = await _transport.GetConnectionAsync(ct);
                if (!ReferenceEquals(_connection, connection))
                    _handlersRegistered = false;
                _connection = connection;
                // Le hub est [Authorize] : on attache le Bearer OIDC.

                Calls = new CallSignalingClient(_connection);
                if (SupportsVideoCalls)
                    VideoCall = new WebRtcCallViewModel(Calls, settings: _settings) { Room = SelectedRoom ?? "" };
                if (SupportsLanCalls)
                    LanCall = new LanCallViewModel(Calls, settings: _settings) { Room = SelectedRoom ?? "" };
                Calls.Invitation += call =>
                {
                    if ((call.Transport == Yavsc.Abstract.Chat.Calls.CallTransports.LanSrtp && !SupportsLanCalls) ||
                        (call.Transport != Yavsc.Abstract.Chat.Calls.CallTransports.LanSrtp && !SupportsVideoCalls))
                        PostForCurrentSession(() =>
                            Log($"Invitation audio-vidéo de {call.Caller} dans {call.RoomName} ; transport non disponible sur cette plateforme."));
                };
                Calls.Ended += call => PostForCurrentSession(() =>
                    Log($"Appel terminé : {call.Reason}."));

                if (!_handlersRegistered)
                {
                    // 1. Ecoute des notifications globales (notifyUser)
                    _connection.On<string, string, string>("notifyUser", (type, user, msg) =>
                    {
                        PostForCurrentSession(() =>
                        {
                            Log($"[{type}] {user}: {msg}");
                        });
                    });

                    // 2. Ecoute des notifications de salon (notifyRoom)
                    _connection.On<string, string, string>("notifyRoom", (type, room, user) =>
                    {
                        PostForCurrentSession(() =>
                        {
                            if (_messagesByRoom.TryGetValue(room, out var roomLog))
                                roomLog.Add($"*{user} a interagi avec le salon {room} (Action: {type})");
                            else
                                Log($"*{user} a interagi avec le salon {room} (Action: {type})");
                            UpdateRoomUsers(type, room, user);
                        });
                    });

                    // WithAutomaticReconnect ne lève pas d'exception en cas de
                    // perte réseau : l'état passe par Reconnecting/Closed. Sans
                    // ces handlers, IsConnected resterait optimiste et Join()
                    // échouerait avec "connection is not active".
                    _connection.Reconnecting += error =>
                    {
                        PostForCurrentSession(() =>
                        {
                            IsConnected = false;
                            Status = new StatusNotice($"Connexion perdue ({error?.Message ?? "réseau"}), reconnexion...", StatusSeverity.Error,
                                new RelayCheckCommand("Déconnecter", async () => await DisconnectAsync()));
                        });
                        return Task.CompletedTask;
                    };
                    _connection.Reconnected += _ =>
                    {
                        PostForCurrentSession(() =>
                        {
                            IsConnected = true;
                            Status = new StatusNotice("Reconnecté au Hub SignalR", StatusSeverity.Info,
                                new RelayCheckCommand("Déconnecter", async () => await DisconnectAsync()));
                        });
                        return Task.CompletedTask;
                    };
                    _connection.Closed += error =>
                    {
                        PostForCurrentSession(() =>
                        {
                            IsConnected = false;
                            Status = new StatusNotice($"Déconnecté ({error?.Message ?? "fin de connexion"})", StatusSeverity.Info,
                                new RelayCheckCommand("Connecter", async () => await ConnectAsync()));
                        });
                        return Task.CompletedTask;
                    };
                    _handlersRegistered = true;
                }
                await _transport.StartAsync(ct);
                foreach (var room in _transport.JoinedRooms)
                    OnRoomJoined(room);
                foreach (var delivery in _transport.RecentMessages)
                    OnMessageReceived(delivery);
                IsConnected = true;
                Status = new StatusNotice("Connecté au Hub SignalR", StatusSeverity.Info, new RelayCheckCommand("Déconnecter", async () => await DisconnectAsync()));
            }
            catch (Exception ex)
            {
                Status = new StatusNotice($"Erreur : {ex.Message}", StatusSeverity.Error, new RelayCheckCommand("Reconnecter", async () => await ConnectAsync()));
                IsConnected = false;
            }
        }

        // Alimente JoinedRooms et les collections par salle à partir
        // du ChatRoomInfo renvoyé par le hub (valeur de retour du Join
        // et/ou événement "joint"). Idempotent.
        private void OnRoomJoined(ChatRoomInfo roomInfo)
        {
            // ChatHub.Join renseigne toujours Name (salle existante ou
            // créée) ; un Name nul signalerait un payload inattendu.
            var roomName = roomInfo.Name ?? CurrentRoom;
            if (roomInfo.Name == null)
                Log("OnRoomJoined : nom de salle absent du payload, repli sur la salle demandée.");
            if (!_messagesByRoom.TryGetValue(roomName, out var roomLog))
                roomLog = _messagesByRoom[roomName] = new();
            roomLog.Add($"Salon rejoint avec succès : {roomName}");
            var users = _usersByRoom[roomName] = new();
            foreach (var u in roomInfo.UserNames ?? roomInfo.Users) users.Add(u);
            if (!JoinedRooms.Contains(roomName))
                JoinedRooms.Add(roomName);
            SelectedRoom = roomName;
        }

        private Task CheckConnectionAsync()
        {
            var state = _connection?.State.ToString() ?? "aucune connexion";
            Log($"État de la connexion : {state}");
            return Task.CompletedTask;
        }

        private async Task JoinRoomAsync()
        {
            if (_connection == null || string.IsNullOrWhiteSpace(CurrentRoom)) return;

            // Salle déjà jointe : on se contente de la mettre en focus.
            if (JoinedRooms.Contains(CurrentRoom))
            {
                SelectedRoom = CurrentRoom;
                return;
            }

            // InvokeCoreAsync exige une connexion active : avec la
            // reconnexion automatique, l'état peut être Reconnecting au
            // moment de l'appel. On tente une (re)connexion si nécessaire.
            if (_connection.State != HubConnectionState.Connected)
            {
                Log($"Connexion non active ({_connection.State}), tentative de reconnexion...");
                try
                {
                    await _transport!.StartAsync();
                }
                catch (Exception ex)
                {
                    Log($"Reconnexion impossible : {ex.Message}");
                    return;
                }
            }

            try
            {
                // Appel de la méthode publique 'Join' du Hub (renvoie un ChatRoomInfo)
                var info = await _transport!.JoinAsync(CurrentRoom);
                if (info == null)
                {
                    Log("Erreur lors de la validation du salon par le serveur.");
                }
                else
                {
                    // On alimente le modèle depuis la valeur de retour,
                    // sans attendre l'événement "joint".
                    OnRoomJoined(info);
                }
            }
            catch (Exception ex)
            {
                Log($"Erreur Join: {ex.Message}");
            }
        }
    }
}
