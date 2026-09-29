using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;

namespace PostIt.ViewModels.Chat
{

    public partial class ChatViewModel : ViewModelBase
    {
        private HubConnection? _connection;

        [ObservableProperty]
        public partial string ServerUrl { get; protected set; } = "http://localhost:5000/chathub";


        [ObservableProperty]
        public partial string CurrentRoom { get; set; } =  "General";

        [ObservableProperty]
        public partial string NickName { get; set; } = "";

        [ObservableProperty]
        public partial StatusNotice Status { get; set; }
                = new StatusNotice("Déconnecté", StatusSeverity.Info);

        [ObservableProperty]
        public partial bool IsConnected { get; set; } = false;

        // Listes dynamiques pour l'UI
        public ObservableCollection<string> Messages { get; } = new();
        public ObservableCollection<string> ConnectedUsers { get; } = new();

        // Commandes pour les boutons
        [RelayCommand]
        public async Task ConnectCommand() => await ConnectAsync();

        [RelayCommand]
        public async Task JoinRoomCommand() => await JoinRoomAsync();

        [RelayCommand]
        public async Task ChangeNickCommand() => await ChangeNickAsync();

        public override bool CanNavigateNext { get; protected set; } = false;
        public override bool CanNavigatePrevious { get; protected set; } = true;

        private async Task ConnectAsync()
        {
            try
            {
                Status = new StatusNotice("Connexion en cours...", StatusSeverity.Info);

                _connection = new HubConnectionBuilder()
                    .WithUrl(ServerUrl)
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
                        ConnectedUsers.Clear();
                        foreach (var u in roomInfo.Users) ConnectedUsers.Add(u);
                    });
                });

                await _connection.StartAsync();
                IsConnected = true;
                Status = new StatusNotice("Connecté au Hub SignalR", StatusSeverity.Info);
            }
            catch (Exception ex)
            {
                Status = new StatusNotice($"Erreur : {ex.Message}", StatusSeverity.Error);
                IsConnected = false;
            }
        }

        private async Task JoinRoomAsync()
        {
            if (_connection == null || string.IsNullOrWhiteSpace(CurrentRoom)) return;

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

        private async Task ChangeNickAsync()
        {
            if (_connection == null || string.IsNullOrWhiteSpace(NickName)) return;

            try
            {
                // Appel de la méthode publique 'Nick' du Hub (void)
                await _connection.InvokeAsync("Nick", NickName);
                Messages.Add($"Demande de changement de pseudo vers '{NickName}' envoyée.");
            }
            catch (Exception ex)
            {
                Messages.Add($"Erreur Nick: {ex.Message}");
            }
        }
    }
}
