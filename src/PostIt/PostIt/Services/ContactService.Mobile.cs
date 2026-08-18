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
/// Mobile implementation backed by MAUI Essentials
/// <c>Contacts.Default</c>.
///
/// <para>Compiled only for ANDROID and IOS. On desktop targets,
/// see <c>ContactService.Desktop.cs</c> (the stub that wins at
/// compile time).</para>
///
/// <para>Note: at runtime, this class throws
/// <c>NotImplementedInReferenceAssemblyException</c> unless
/// the host application project also references the
/// platform-specific Microsoft.Maui.Essentials implementation
/// (typically <c>PostIt.Android</c>). On iOS the same is
/// required via <c>PostIt.iOS</c>. On desktop the stub is used
/// and this file is excluded.</para>
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

            // Carry the per-contact email list as-is. A real
            // device contact can carry several addresses (home /
            // work / other); the UI use case ("invite / add to a
            // circle") can then decide which address to use, or
            // let the user pick. The platform-neutral ContactDto
            // shape is intentionally richer than the Yavsc
            // directory's single-Email shape — the two flows
            // answer different questions.
            var result = new List<ContactDto>(contacts.Count);
            foreach (var c in contacts)
            {
                var emails = ExtractEmails(c.Emails);
                result.Add(new ContactDto(
                    c.Id,
                    c.DisplayName ?? string.Empty,
                    emails));
            }
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ContactService: {ex.Message}");
            return Array.Empty<ContactDto>();
        }
    }

    private static IReadOnlyList<string> ExtractEmails(IEnumerable<EmailAddress>? emails)
    {
        if (emails is null) return Array.Empty<string>();
        var list = new List<string>();
        foreach (var e in emails)
        {
            if (!string.IsNullOrEmpty(e.EmailAddress))
                list.Add(e.EmailAddress);
        }
        return list;
    }
}
#endif
