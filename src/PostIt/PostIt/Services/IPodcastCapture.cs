using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace PostIt.Services;

public sealed record CaptureDevice(string Name, string Value);
public sealed record PodcastCaptureDevices(
    IReadOnlyList<CaptureDevice> Cameras, IReadOnlyList<CaptureDevice> Microphones);

/// <summary>A running encoder publishing closed segments and an atomic index.m3u8.</summary>
public interface IPodcastCapture : IAsyncDisposable
{
    Task Completion { get; }
    Control? Preview => null;
    Task StopAsync();
}
