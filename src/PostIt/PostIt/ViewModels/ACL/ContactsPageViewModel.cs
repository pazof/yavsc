using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yavsc.Api.Client;
using Yavsc.Api.Client.Dtos;

namespace PostIt.ViewModels;

public sealed record ContactEntry(string Id, string UserName, string DisplayName, string Circles);

public partial class ContactsPageViewModel(CircleApiClient client) : ViewModelBase
{
    public ObservableCollection<ContactEntry> Contacts { get; } = [];

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string Status { get; set; } = "Pret.";

    public override bool CanNavigateNext { get; protected set; } = false;
    public override bool CanNavigatePrevious { get; protected set; } = true;

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var circles = await client.GetMyCirclesAsync()
                ?? throw new InvalidOperationException("Le serveur n'a pas renvoye les cercles.");
            var members = new List<(CircleDto Circle, CircleMemberDto Member)>();
            foreach (var circle in circles)
            {
                var list = await client.GetMembersAsync(circle.Id)
                    ?? throw new InvalidOperationException($"Le cercle {circle.Name} n'est plus disponible.");
                members.AddRange(list.Select(member => (circle, member)));
            }
            var contacts = members.GroupBy(item => item.Member.Id, StringComparer.Ordinal)
                .Select(group =>
                {
                    var member = group.First().Member;
                    if (string.IsNullOrWhiteSpace(member.Id) || string.IsNullOrWhiteSpace(member.UserName))
                        throw new InvalidOperationException("Un membre de cercle n'a pas d'identifiant ou de pseudo.");
                    return new ContactEntry(member.Id, member.UserName,
                        string.IsNullOrWhiteSpace(member.FullName) ? member.UserName : member.FullName,
                        string.Join(", ", group.Select(item => item.Circle.Name).Distinct().OrderBy(name => name)));
                })
                .OrderBy(contact => contact.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();
            Contacts.Clear();
            foreach (var contact in contacts) Contacts.Add(contact);
            Status = $"{Contacts.Count} contact(s).";
        }
        catch (Exception error)
        {
            Contacts.Clear();
            Status = $"Impossible de charger les contacts : {error.Message}";
        }
        finally { IsBusy = false; }
    }
}
