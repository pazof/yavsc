#if !ANDROID && !IOS
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PostIt.Services;

/// <summary>
/// Desktop stub for IContactService.
///
/// On desktop targets (Linux, macOS, Windows) MAUI Essentials
/// Contacts.Default throws NotImplementedInReferenceAssemblyException,
/// so we short-circuit with an empty list rather than trying to
/// call into the portable facade at runtime.
///
/// Future provider plug-ins (Google Contacts API, Exchange EWS,
/// CardDAV) can either replace this stub on a per-OS basis or
/// live behind their own IContactService implementation that the
/// DI container selects by configuration.
/// </summary>
public sealed class ContactService : IContactService
{
    public Task<IReadOnlyList<ContactDto>> GetDeviceContactsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ContactDto>>(Array.Empty<ContactDto>());
}
#endif
