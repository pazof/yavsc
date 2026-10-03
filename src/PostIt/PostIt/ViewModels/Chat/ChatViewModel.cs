using System;
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
        public partial bool HasJoinedRoom { get; set; } = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        public partial string MessageToSend { get; set; } = "";

        // Listes dynamiques pour l'UI
        public ObservableCollection<string> Messages { get; } = new();
        public ObservableCollection<string> ConnectedUsers { get; } = new();

        // Commandes pour les boutons
        [RelayCommand]
        public async Task ConnectCommand() => await ConnectAsync();

        [RelayCommand]
        public async Task JoinRoomCommand() => await JoinRoomAsync();

        // Le serveur (ChatHub.Send) refuse tout message d'un utilisateur
        // qui n'a pas joint le salon ("NoJoinNoSend") : on garde la
        // commande inactive tant que le Join n'a pas abouti.
        private bool CanSendMessage() =>
            IsConnected && HasJoinedRoom && !string.IsNullOrWhiteSpace(MessageToSend);

        [RelayCommand(CanExecute = nameof(CanSendMessage))]
        private async Task SendMessageAsync()
        {
            if (_connection == null) return;
            var text = MessageToSend.Trim();
            try
            {
                await _connection.InvokeAsync("Send", CurrentRoom, text);
                MessageToSend = string.Empty;
            }
            catch (Exception ex)
            {
                Messages.Add($"Erreur d'envoi : {ex.Message}");
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
            HasJoinedRoom = false;
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
                        Messages.Add($"[{type}] {user}: {msg}"));
                });

                // 2. Ecoute des notifications de salon (notifyRoom)
                _connection.On<string, string, string>("notifyRoom", (type, room, user) =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        Messages.Add($"*{user} a interagi avec le salon {room} (Action: {type})");
                        if (type == "UserJoin" && !ConnectedUsers.Contains(user))
                            ConnectedUsers.Add(user);
                    });
                });

                // 3. Réception de l'état complet suite au Join réussi (joint)
                _connection.On<ChatRoomInfo>("joint", (roomInfo) =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        Messages.Add($"Salon rejoint avec succès : {roomInfo.Name}");
                        HasJoinedRoom = true;
                        ConnectedUsers.Clear();
                        foreach (var u in roomInfo.Users) ConnectedUsers.Add(u);
                    });
                });

                // 4. Réception des messages du salon (diffusés par ChatHub.Send)
                _connection.On<ChatMessage>("ReceiveMessage", (msg) =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        Messages.Add($"[{msg.Room}] {msg.Name} : {msg.Message}"));
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
                _connection.Reconnected += _ =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        IsConnected = true;
                        Status = new StatusNotice("Reconnecté au Hub SignalR", StatusSeverity.Info,
                            new RelayCheckCommand("Déconnecter", async () => await DisconnectAsync()));
                    });
                    return Task.CompletedTask;
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

        private Task CheckConnectionAsync()
        {
            var state = _connection?.State.ToString() ?? "aucune connexion";
            Messages.Add($"État de la connexion : {state}");
            return Task.CompletedTask;
        }

        private async Task JoinRoomAsync()
        {
            if (_connection == null || string.IsNullOrWhiteSpace(CurrentRoom)) return;

            // InvokeCoreAsync exige une connexion active : avec la
            // reconnexion automatique, l'état peut être Reconnecting au
            // moment de l'appel. On tente une (re)connexion si nécessaire.
            if (_connection.State != HubConnectionState.Connected)
            {
                Messages.Add($"Connexion non active ({_connection.State}), tentative de reconnexion...");
                try
                {
                    await _connection.StartAsync();
                }
                catch (Exception ex)
                {
                    Messages.Add($"Reconnexion impossible : {ex.Message}");
                    return;
                }
            }

            try
            {
                // Appel de la méthode publique 'Join' du Hub (renvoie un ChatRoomInfo)
                var info = await _connection.InvokeAsync<ChatRoomInfo>("Join", CurrentRoom);
                if (info == null)
                {
                    Messages.Add("Erreur lors de la validation du salon par le serveur.");
                }
            }
            catch (Exception ex)
            {
                Messages.Add($"Erreur Join: {ex.Message}");
            }
        }
    }
}
