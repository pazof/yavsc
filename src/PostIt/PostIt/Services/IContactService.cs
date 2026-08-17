using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PostIt.Services;

/// <summary>
/// Abstraction over device contact providers (MAUI Essentials on mobile,
/// future Google/Exchange/IMAP providers).
///
/// Implementations live next to this file in platform-conditional
/// source files: ContactService.Mobile.cs (ANDROID/IOS) and
/// ContactService.Desktop.cs (everything else).
/// </summary>
public interface IContactService
{
    Task<IReadOnlyList<ContactDto>> GetDeviceContactsAsync(CancellationToken ct = default);
}

/// <summary>
/// Platform-neutral contact DTO. Source-of-truth shape for the UI layer;
/// concrete providers (MAUI Essentials today, Google Contacts API later)
/// map to this type.
/// </summary>
public sealed record ContactDto(
    string Id,
    string DisplayName,
    IReadOnlyList<string> Emails);
