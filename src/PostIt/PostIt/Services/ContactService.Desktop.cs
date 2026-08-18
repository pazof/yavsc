#if !ANDROID && !IOS
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PostIt.Services;

/// <summary>
/// Desktop stub for <see cref="IContactService"/>.
///
/// <para>The desktop has no equivalent of the mobile address
/// book (no <c>Contacts.Default</c>, no CardDAV out of the
/// box). Rather than synthesise a list from a different
/// source, this provider returns an empty list and lets the
/// UI render an honest "no local contacts on this platform"
/// message.</para>
///
/// <para>If desktop users want to invite people who aren't
/// Yavsc members, that flow goes through a separate path
/// (manual email entry + invitation endpoint) — not through
/// <see cref="IContactService"/>. Finding existing Yavsc
/// members is <see cref="IUserDirectory"/>'s job, not this
/// one's.</para>
///
/// <para>Future CardDAV / Google Contacts / Exchange
/// providers can plug in here as additional
/// <see cref="IContactService"/> implementations selected
/// from DI by configuration.</para>
/// </summary>
public sealed class ContactService : IContactService
{
    public Task<IReadOnlyList<ContactDto>> GetDeviceContactsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ContactDto>>(Array.Empty<ContactDto>());
}
#endif
