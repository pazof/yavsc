using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using PostIt.Models;

namespace PostIt.Services;

public sealed class BlogApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _serializerOptions;

    public BlogApiClient(string baseUrl, string? bearerToken = null)
        : this(CreateHttpClient(baseUrl, bearerToken))
    {
    }

    public BlogApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private static HttpClient CreateHttpClient(string baseUrl, string? bearerToken)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/")
        };

        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken.Trim());
        }

        return client;
    }

    public async Task<List<BlogPost>> GetPostsAsync(int start = 0, int take = 25)
    {
        var result = await _httpClient.GetFromJsonAsync<List<BlogPost>>($"api/blog?start={start}&take={take}", _serializerOptions).ConfigureAwait(false);
        return result ?? new List<BlogPost>();
    }

    public Task<BlogPost?> GetPostAsync(long id)
        => _httpClient.GetFromJsonAsync<BlogPost>($"api/blog/{id}", _serializerOptions);

    public async Task<BlogPost?> CreatePostAsync(BlogPost post)
    {
        var response = await _httpClient.PostAsJsonAsync("api/blog", post, _serializerOptions).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BlogPost>(_serializerOptions).ConfigureAwait(false);
    }

    public async Task UpdatePostAsync(long id, BlogPost post)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/blog/{id}", post, _serializerOptions).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeletePostAsync(long id)
    {
        var response = await _httpClient.DeleteAsync($"api/blog/{id}").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
