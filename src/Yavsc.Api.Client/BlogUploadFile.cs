namespace Yavsc.Api.Client;

/// <summary>
/// Buffered file payload for multipart blog uploads.
/// </summary>
public sealed record BlogUploadFile(string FileName, byte[] Content, string? ContentType = null);
