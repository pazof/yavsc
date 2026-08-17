#if ANDROID || IOS
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace PostIt.Services;

/// <summary>
/// Mobile implementation backed by MAUI Essentials Contacts.Default.
///
/// Compiled only for ANDROID and IOS. On desktop targets, see
/// ContactService.Desktop.cs (the stub that wins at compile time).
///
/// Note: at runtime, this class throws
/// NotImplementedInReferenceAssemblyException unless the host
/// application project also references the platform-specific
/// Microsoft.Maui.Essentials implementation (typically the
/// PostIt.Android project). On iOS the same is required via
/// PostIt.iOS. On desktop the stub is used and this file is excluded.
/// </summary>
public sealed class ContactService : IContactService
{
    public ObservableCollection<ContactDto> Contacts { get; } = new();

    public async Task<IReadOnlyList<ContactDto>> GetDeviceContactsAsync(CancellationToken ct = default)
    {
        if (DeviceInfo.Current.Platform == DevicePlatform.Unknown)
            return Array.Empty<ContactDto>();

        try
        {
            var status = await Permissions.RequestAsync<Permissions.ContactsRead>();
            if (status != PermissionStatus.Granted)
                return Array.Empty<ContactDto>();

            var contacts = await Contacts.Default.GetAllAsync();
            if (contacts is null) return Array.Empty<ContactDto>();

            // Flatten the per-contact email list down to one
            // primary email. The platform-neutral ContactDto only
            // carries one; the use case ("invite / add to a
            // circle") only needs one. The first non-empty entry
            // wins.
            Contacts.Clear();
            foreach (var c in contacts)
            {
                var email = FlattenPrimaryEmail(c.Emails);
                Contacts.Add(new ContactDto(c.Id, c.DisplayName ?? string.Empty, email));
            }
            return Contacts.ToArray();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ContactService: {ex.Message}");
            return Array.Empty<ContactDto>();
        }
    }

    public Task SearchAsync(string query, CancellationToken ct = default)
        => throw new PlatformNotSupportedException(
            "SearchAsync is not supported on mobile — use GetDeviceContactsAsync " +
            "to load the local address book. The network search lives on the " +
            "desktop service, which queries the central user-search endpoint.");

    private static string? FlattenPrimaryEmail(IEnumerable<EmailAddress>? emails)
    {
        if (emails is null) return null;
        foreach (var e in emails)
        {
            if (!string.IsNullOrEmpty(e.EmailAddress))
                return e.EmailAddress;
        }
        return null;
    }
}
#endif