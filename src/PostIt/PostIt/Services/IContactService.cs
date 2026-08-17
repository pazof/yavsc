using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace PostIt.Services;

/// <summary>
/// Abstraction over device contact providers (MAUI Essentials on
/// mobile, the central /api/user-search endpoint on desktop).
///
/// Implementations live next to this file in platform-conditional
/// source files: ContactService.Mobile.cs (ANDROID/IOS) and
/// ContactService.Desktop.cs (everything else).
/// </summary>
public interface IContactService
{
    /// <summary>
    /// Returns the contacts known so far. On mobile this is the
    /// full device address book (after permission grant); on
    /// desktop this is the in-memory cache populated by previous
    /// <see cref="SearchAsync"/> calls — empty until the user
    /// has searched for something.
    /// </summary>
    Task<IReadOnlyList<ContactDto>> GetDeviceContactsAsync(CancellationToken ct = default);

    /// <summary>
    /// On desktop: hits <c>GET /api/user-search?q=…</c> and
    /// appends matching users to the in-memory cache exposed via
    /// <see cref="Contacts"/>. On mobile: throws
    /// <see cref="PlatformNotSupportedException"/> — the mobile
    /// provider uses the device-local address book, not a
    /// network search.
    /// </summary>
    Task SearchAsync(string query, CancellationToken ct = default);

    /// <summary>
    /// Live view of the in-memory contact cache. UI binds to
    /// this directly for a \"search results\" panel; on mobile
    /// implementations this is populated eagerly by
    /// <see cref="GetDeviceContactsAsync"/>.
    /// </summary>
    ObservableCollection<ContactDto> Contacts { get; }
}

/// <summary>
/// Platform-neutral contact DTO. Source-of-truth shape for the UI
/// layer; concrete providers (MAUI Essentials on mobile,
/// UserSearchClient on desktop) map to this type.
///
/// <para><c>Email</c> is a single string on purpose: the central
/// search endpoint returns one email per user, and the UI use
/// case is \"pick someone to invite / add to a circle\", which
/// never needs more than one. Multi-email contacts on mobile
/// flatten to the primary address (first non-empty).</para>
/// </summary>
public sealed record ContactDto(
    string Id,
    string DisplayName,
    string? Email);