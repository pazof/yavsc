using System.Net;
using System.Text.Json;
using PostIt.ViewModels;
using Yavsc.Api.Client;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
public sealed class ContactsPageViewModelTests
{
    [Fact]
    public async Task Contacts_are_deduplicated_and_show_all_their_circles()
    {
        await using var api = new FakeApi();
        var vm = new ContactsPageViewModel(new CircleApiClient(api, "https://example.test/"));
        await vm.RefreshAsync();
        var contact = Assert.Single(vm.Contacts);
        Assert.Equal("bob-id", contact.Id);
        Assert.Equal("Bob", contact.DisplayName);
        Assert.Equal("Amis, Travail", contact.Circles);
    }

    [Fact]
    public async Task Missing_circle_is_reported_instead_of_showing_incomplete_contacts()
    {
        await using var api = new FakeApi { MissingCircle = true };
        var vm = new ContactsPageViewModel(new CircleApiClient(api, "https://example.test/"));
        await vm.RefreshAsync();
        Assert.Empty(vm.Contacts);
        Assert.Contains("n'est plus disponible", vm.Status);
        Assert.False(vm.IsBusy);
    }

    private sealed class FakeApi : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public bool MissingCircle { get; init; }

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            string json = path switch
            {
                "circle" => """[{"id":1,"name":"Travail"},{"id":2,"name":"Amis"}]""",
                "circle/1/members" => """[{"id":"bob-id","userName":"bob","fullName":"Bob"}]""",
                "circle/2/members" when MissingCircle => "null",
                "circle/2/members" => """[{"id":"bob-id","userName":"bob","fullName":"Bob"}]""",
                _ => throw new HttpRequestException("Unexpected path", null, HttpStatusCode.NotFound)
            };
            return Task.FromResult(JsonSerializer.Deserialize<T>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!);
        }

        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public ValueTask DisposeAsync() { Http.Dispose(); return ValueTask.CompletedTask; }
    }
}
