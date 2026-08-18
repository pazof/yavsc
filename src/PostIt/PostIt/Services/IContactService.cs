using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PostIt.Services;

/// <summary>
/// Abstraction over the device-local address book. Used by
/// the "invite someone" flow to enumerate people the user
/// already has in their phone — including people who have
/// never heard of Yavsc.
///
/// <para>Distinct from <see cref="IUserDirectory"/>, which
/// reads the central Yavsc user table. A device contact may
/// not have a Yavsc account; a directory entry always does.
/// The two are exposed as separate interfaces so a UI that
/// needs both can take both by constructor injection and
/// present them under separate sections (e.g. "Contacts from
/// your phone" vs "Yavsc members").</para>
///
/// <para>Implementations live next to this file in
/// platform-conditional source files:
/// <c>ContactService.Mobile.cs</c> (ANDROID/IOS) and
/// <c>ContactService.Desktop.cs</c> (everything else). On
/// desktop the implementation is a stub that returns an
/// empty list: the desktop has no equivalent of the mobile
/// address book, and inviting from a desktop is a separate
/// flow.</para>
/// </summary>
public interface IContactService
{
    /// <summary>
    /// Read the device address book. Returns the contacts
    /// known to the local provider; on desktop (no local
    /// provider) this is always an empty list.
    /// </summary>
    Task<IReadOnlyList<ContactDto>> GetDeviceContactsAsync(CancellationToken ct = default);
}

/// <summary>
/// Platform-neutral contact DTO. Source-of-truth shape for
/// the UI layer; concrete providers (MAUI Essentials on
/// mobile) map to this type.
///
/// <para><c>Emails</c> is a list on purpose: a real device
/// contact may carry several addresses (home / work / other).
/// The UI use case ("invite / add to a circle") can then
/// decide which address to use, or let the user pick. This
/// is intentionally richer than the Yavsc directory's
/// single-<c>Email</c> shape — the two flows answer different
/// questions and shouldn't be flattened onto the same
/// wire.</para>
/// </summary>
public sealed record ContactDto(
    string Id,
    string DisplayName,
    IReadOnlyList<string> Emails);
