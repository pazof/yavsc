using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

        // Le paramètre optionnel permet l'instanciation design-time
        // (Design.DataContext dans ChatPage.axaml) sans conteneur DI.
        public ChatViewModel(YavscApiClient? api = null)
        {
            var app = App.Current as App;
            _settings = app!.ServiceProvider!.GetRequiredService<Settings>(); // Assurez-vous que _settings est initialisé avant utilisation.
            _api = api;
            Status = new StatusNotice("Déconnecté", StatusSeverity.Info,
            new RelayCheckCommand("Connecter", async () => await ConnectAsync()));
            FocusedMessages = _systemMessages;
        }

        private HubConnection? _connection;

        [ObservableProperty]
        public partial string CurrentRoom { get; set; } =  "General";

        [ObservableProperty]
        public partial string NickName { get; set; } = "";

        [ObservableProperty]
        public partial StatusNotice Status { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
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

        partial void OnSelectedRoomChanged(string? value)
        {
            FocusedMessages = value != null && _messagesByRoom.TryGetValue(value, out var msgs)
                ? msgs : _systemMessages;
            FocusedUsers = value != null && _usersByRoom.TryGetValue(value, out var users)
                ? users : _noUsers;
        }

        // Trace une notification système dans la vue courante
        // (et dans le journal global pour consultation ultérieure).
        private void Log(string line)
        {
            _systemMessages.Add(line);
            if (!ReferenceEquals(FocusedMessages, _systemMessages))
                FocusedMessages.Add(line);
        }

        // Commandes pour les boutons
        [RelayCommand]
        public async Task ConnectCommand() => await ConnectAsync();

        [RelayCommand]
        public async Task JoinRoomCommand() => await JoinRoomAsync();

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

        private async Task DisconnectAsync()
        {
            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }
            IsConnected = false;
            SelectedRoom = null;
            JoinedRooms.Clear();
            _messagesByRoom.Clear();
            _usersByRoom.Clear();
            _systemMessages.Clear();
            Status = new StatusNotice("Déconnecté", StatusSeverity.Info,
             new RelayCheckCommand("Connecter", async () => await ConnectAsync()));
        }

        private async Task ConnectAsync()
        {
            try
            {
                Status = new StatusNotice("Connexion en cours...", StatusSeverity.Info,
                 new RelayCheckCommand("Vérifier la connexion", async () => await CheckConnectionAsync()));

                if (_api == null || !_api.HasValidSession)
                {
                    Status = new StatusNotice("Session expirée : reconnectez-vous avant d'ouvrir le chat.", StatusSeverity.Error,
                    new RelayCheckCommand("Reconnecter", async () => await ConnectAsync()));
                    return;
                }

                // Le hub est mappé à la racine du service ("/chathub"),
                // pas sous le préfixe versionné "/api/v1" : on le retire.
                var hubBase = _settings.ApiUrl.TrimEnd('/');
                if (hubBase.EndsWith("/api/v1", StringComparison.OrdinalIgnoreCase))
                    hubBase = hubBase[..^"/api/v1".Length];
                var hubUrl = hubBase + "/" + Constants.ChatHubPath;

                _connection = new HubConnectionBuilder()
                    .WithUrl(hubUrl,

                    options =>
                    {
                        // Le hub est [Authorize] : on attache le Bearer OIDC.
                        // AccessTokenProvider est rappelé à chaque reconnexion,
                        // donc on tente un refresh silencieux à chaque appel.
                        // Un refresh mort (invalid_grant) ne doit pas faire
                        // échouer StartAsync avec une exception interne : on
                        // renvoie le token courant (éventuellement expiré) et
                        // le serveur répondra un 401 propre, surfacé à l'UI.
                        options.AccessTokenProvider = async () =>
                        {
                            await _api.TrySilentLoginAsync();
                            return _api.CurrentAccessToken;
                        };
                    })
                    .WithAutomaticReconnect()
                    .Build();

                // 1. Ecoute des notifications globales (notifyUser)
                _connection.On<string, string, string>("notifyUser", (type, user, msg) =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        Log($"[{type}] {user}: {msg}"));
                });

                // 2. Ecoute des notifications de salon (notifyRoom)
                _connection.On<string, string, string>("notifyRoom", (type, room, user) =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        if (_messagesByRoom.TryGetValue(room, out var roomLog))
                            roomLog.Add($"*{user} a interagi avec le salon {room} (Action: {type})");
                        else
                            Log($"*{user} a interagi avec le salon {room} (Action: {type})");
                        if (type == "UserJoin" && _usersByRoom.TryGetValue(room, out var users)
                            && !users.Contains(user))
                            users.Add(user);
                    });
                });

                // 3. Réception de l'état complet suite au Join réussi (joint)
                _connection.On<ChatRoomInfo>("joint", (roomInfo) =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => OnRoomJoined(roomInfo));
                });

                // 4. Réception des messages de salon (diffusés par ChatHub.Send)
                _connection.On<ChatMessage>("ReceiveMessage", (msg) =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        var room = msg.Room ?? "?";
                        if (!_messagesByRoom.TryGetValue(room, out var roomLog))
                            roomLog = _messagesByRoom[room] = new();
                        roomLog.Add($"{msg.Name} : {msg.Message}");
                    });
                });

                await _connection.StartAsync();
                IsConnected = true;
                Status = new StatusNotice("Connecté au Hub SignalR", StatusSeverity.Info, new RelayCheckCommand("Déconnecter", async () => await DisconnectAsync()));

                // WithAutomaticReconnect ne lève pas d'exception en cas de
                // perte réseau : l'état passe par Reconnecting/Closed. Sans
                // ces handlers, IsConnected resterait optimiste et Join()
                // échouerait avec "connection is not active".
                _connection.Reconnecting += error =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        IsConnected = false;
                        Status = new StatusNotice($"Connexion perdue ({error?.Message ?? "réseau"}), reconnexion...", StatusSeverity.Error,
                            new RelayCheckCommand("Déconnecter", async () => await DisconnectAsync()));
                    });
                    return Task.CompletedTask;
                };
                _connection.Reconnected += async _ =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        IsConnected = true;
                        Status = new StatusNotice("Reconnecté au Hub SignalR", StatusSeverity.Info,
                            new RelayCheckCommand("Déconnecter", async () => await DisconnectAsync()));
                    });
                    // La reconnexion crée une nouvelle ConnectionId côté
                    // serveur : l'appartenance aux salons est perdue, il
                    // faut rejoindre à nouveau le salon courant.
                    if (!string.IsNullOrWhiteSpace(CurrentRoom))
                        await JoinRoomAsync();
                };
                _connection.Closed += error =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        IsConnected = false;
                        Status = new StatusNotice($"Déconnecté ({error?.Message ?? "fin de connexion"})", StatusSeverity.Info,
                            new RelayCheckCommand("Connecter", async () => await ConnectAsync()));
                    });
                    return Task.CompletedTask;
                };
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
            foreach (var u in roomInfo.Users) users.Add(u);
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
                    await _connection.StartAsync();
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
                var info = await _connection.InvokeAsync<ChatRoomInfo>("Join", CurrentRoom);
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
