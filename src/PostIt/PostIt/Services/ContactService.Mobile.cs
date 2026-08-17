#if ANDROID || IOS
using System;
using System.Collections.Generic;
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

            var result = new List<ContactDto>();
            foreach (var c in contacts)
            {
                var emails = new List<string>();
                if (c.Emails is not null)
                {
                    foreach (var e in c.Emails)
                    {
                        if (!string.IsNullOrEmpty(e.EmailAddress))
                            emails.Add(e.EmailAddress);
                    }
                }
                result.Add(new ContactDto(c.Id, c.DisplayName ?? string.Empty, emails));
            }
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ContactService: {ex.Message}");
            return Array.Empty<ContactDto>();
        }
    }
}
#endif
