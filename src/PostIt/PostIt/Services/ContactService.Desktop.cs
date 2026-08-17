#if !ANDROID && !IOS
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Api.Client;
using Yavsc.Api.Client.Dtos;

namespace PostIt.Services;

/// <summary>
/// Desktop implementation of <see cref="IContactService"/> backed
/// by the central <c>/api/user-search</c> endpoint
/// (<see cref="UserSearchClient"/>).
///
/// <para>Desktop has no equivalent of the mobile address book
/// (no Contacts.Default, no CardDAV out of the box), so the
/// address book is built on demand from the Yavsc user table.
/// Results are accumulated in an in-memory cache exposed as
/// <see cref="Contacts"/>; the cache is process-lifetime only
/// — there's no persistence layer.</para>
///
/// <para>This is the consumer that closes the loop with the
/// user-search endpoint landed on the server in commit 6
/// (<c>b3056f1c</c>) and the client in commit 7
/// (<c>6e7e0414</c>).</para>
/// </summary>
public sealed class ContactService : IContactService
{
    private readonly UserSearchClient _client;

    public ObservableCollection<ContactDto> Contacts { get; } = new();

    public ContactService(UserSearchClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public Task<IReadOnlyList<ContactDto>> GetDeviceContactsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ContactDto>>(Contacts.ToArray());

    public async Task SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            // Clear the cache to mirror an empty result. The
            // address-book UX treats an empty query as "start
            // over".
            Contacts.Clear();
            return;
        }

        var results = await _client.SearchAsync(query: query, ct: ct).ConfigureAwait(false);
        if (results is null) return;

        // Append the search results to the cache. We don't
        // de-dupe across searches — the simplest behaviour, and
        // matches what users expect from a search panel ("show
        // me what came back"). Callers wanting a single list
        // can re-render Contacts on the next query.
        foreach (var u in results)
        {
            Contacts.Add(new ContactDto(
                Id: u.Id,
                DisplayName: u.FullName ?? u.UserName,
                Email: u.Email));
        }
    }
}
#endif