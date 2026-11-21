using System.Net.Http;
using System.Text;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public class UserFilesApiClientTests
{
    [Fact]
    public async Task Upload_from_file_streams_video_to_screencast_directory()
    {
        string filePath = Path.Combine(Path.GetTempPath(), $"postit-{Guid.NewGuid():N}.mp4");
        byte[] expected = [0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70];
        await File.WriteAllBytesAsync(filePath, expected, TestContext.Current.CancellationToken);

        try
        {
            var api = new UploadApi();
            var client = new UserFilesApiClient(api, "https://blogs.example/api/v1/");

            var received = await client.UploadAsync(
                "screencast",
                filePath,
                "video/mp4",
                TestContext.Current.CancellationToken);

            Assert.Single(received);
            Assert.Equal(HttpMethod.Post, api.Method);
            Assert.Equal("fs/screencast", api.Path);
            string multipart = Encoding.UTF8.GetString(api.MultipartBody);
            Assert.Contains(Path.GetFileName(filePath), multipart);
            Assert.Contains("video/mp4", multipart);
            Assert.True(api.MultipartBody.AsSpan().IndexOf(expected) >= 0);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    private sealed class UploadApi : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public HttpMethod? Method { get; private set; }
        public string? Path { get; private set; }
        public byte[] MultipartBody { get; private set; } = [];

        public async Task<T> CallAsync<T>(
            HttpMethod method,
            string path,
            Func<HttpContent> contentFactory,
            CancellationToken ct = default)
        {
            Method = method;
            Path = path;
            using HttpContent content = contentFactory();
            MultipartBody = await content.ReadAsByteArrayAsync(ct);
            return (T)(object)new List<FileReceivedDto>
            {
                new() { FileName = Path, ContentType = "video/mp4", Length = MultipartBody.Length }
            };
        }

        public Task<T> CallAsync<T>(
            HttpMethod method,
            string path,
            object? body = null,
            CancellationToken ct = default) => throw new NotSupportedException();

        public Task CallAsync(
            HttpMethod method,
            string path,
            object? body = null,
            CancellationToken ct = default) => throw new NotSupportedException();

        public Task CallAsync(
            HttpMethod method,
            string path,
            Func<HttpContent> contentFactory,
            CancellationToken ct = default) => throw new NotSupportedException();

        public Task<byte[]> DownloadAsync(
            HttpMethod method,
            string path,
            CancellationToken ct = default) => throw new NotSupportedException();

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
