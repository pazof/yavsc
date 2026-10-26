using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace PostIt.Services;

public sealed record CaptureDevice(string Name, string Value);
public sealed record PodcastCaptureDevices(
    IReadOnlyList<CaptureDevice> Cameras, IReadOnlyList<CaptureDevice> Microphones);

/// <summary>A running camera/microphone encoder with an optional native preview.</summary>
public interface IPodcastCapture : IAsyncDisposable
{
    Task Completion { get; }
    Control? Preview => null;
    Task StopAsync();
}
