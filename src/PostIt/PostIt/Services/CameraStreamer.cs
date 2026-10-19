using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using LibVLCSharp.Shared;

using DirectShowLib;

public class CameraStreamer
{
    private readonly LibVLC _libVLC;

    public static LibVLC CreateHeadlessLibVlc()
        => new(
            "--no-xlib",
            "--avcodec-hw=none",
            "--vout=dummy",
            "--aout=dummy",
            "--intf=dummy");

    public static LibVLC CreatePlaybackLibVlc()
    {
        if (!OperatingSystem.IsLinux())
            return new LibVLC();

        var preferHardware = string.Equals(Environment.GetEnvironmentVariable("POSTIT_LIBVLC_ENABLE_HW"), "1",
            StringComparison.OrdinalIgnoreCase);

        if (preferHardware && IsVaapiUsable())
            return new LibVLC();

        // On Linux, a non-hardware display path is still valid for in-app playback as long as
        // the platform is not explicitly forced into a broken VAAPI/VDPAU stack.
        return new LibVLC("--avcodec-hw=none");
    }

    public static LibVLC CreatePreferredLibVlc()
    {
        if (!OperatingSystem.IsLinux())
            return new LibVLC();

        var preferHardware = string.Equals(Environment.GetEnvironmentVariable("POSTIT_LIBVLC_ENABLE_HW"), "1",
            StringComparison.OrdinalIgnoreCase);

        if (preferHardware && IsVaapiUsable())
            return new LibVLC();

        return CreateHeadlessLibVlc();
    }

    private static bool IsVaapiUsable()
    {
        var vaapiDrivers = new[]
        {
            "/usr/lib/x86_64-linux-gnu/dri/nvidia_drv_video.so",
            "/usr/lib/x86_64-linux-gnu/dri/i965_drv_video.so",
            "/usr/lib/x86_64-linux-gnu/dri/i915_drv_video.so",
            "/usr/lib/x86_64-linux-gnu/dri/radeonsi_dri.so",
            "/usr/lib/x86_64-linux-gnu/dri/iris_dri.so",
            "/usr/lib64/dri/nvidia_drv_video.so",
            "/usr/lib64/dri/i965_drv_video.so",
            "/usr/lib64/dri/i915_drv_video.so",
            "/usr/lib64/dri/radeonsi_dri.so",
            "/usr/lib64/dri/iris_dri.so"
        };

        if (!vaapiDrivers.Any(File.Exists))
            return false;

        try
        {
            var processInfo = new ProcessStartInfo("vainfo")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            using var process = Process.Start(processInfo);
            if (process is null)
                return false;

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit(2000);

            var combined = (output + Environment.NewLine + error).Trim();
            return process.ExitCode == 0
                && !string.IsNullOrWhiteSpace(combined)
                && combined.Contains("VA-API version", StringComparison.OrdinalIgnoreCase)
                && !combined.Contains("failed", StringComparison.OrdinalIgnoreCase)
                && !combined.Contains("unknown libva error", StringComparison.OrdinalIgnoreCase)
                && !combined.Contains("va_openDriver() returns -1", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public CameraStreamer(LibVLC libVLC)
    {
        _libVLC = libVLC ?? throw new ArgumentNullException(nameof(libVLC));
    }

    /// <summary>
    /// Récupère la liste des périphériques de capture disponibles sur la machine.
    /// Retourne un dictionnaire : Clé = Nom lisible, Valeur = Identifiant/Chemin système
    /// </summary>
    public Dictionary<string, string> GetCaptureDevices()
    {
        var devices = new Dictionary<string, string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Détection Windows via DirectShow
            DsDevice[] capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            foreach (var device in capDevices)
            {
                using (device)
                    devices.TryAdd(device.Name, device.Name);
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Détection Linux via V4L2 (Video4Linux2)
            if (Directory.Exists("/dev"))
            {
                var videoFiles = Directory.GetFiles("/dev", "video*");
                foreach (var path in videoFiles)
                {
                    // Exemple : Clé = "Caméra (/dev/video0)", Valeur = "/dev/video0"
                    devices.Add($"Caméra ({path})", path);
                }
            }
        }
        else
        {
            throw new PlatformNotSupportedException("OS non supporté pour la capture automatique dans cet exemple.");
        }

        return devices;
    }

    public Dictionary<string, string> GetAudioCaptureDevices()
    {
        var devices = new Dictionary<string, string>();
        if (OperatingSystem.IsWindows())
        {
            foreach (var device in DsDevice.GetDevicesOfCat(FilterCategory.AudioInputDevice))
            {
                using (device)
                    devices.TryAdd(device.Name, device.Name);
            }
        }
        else if (OperatingSystem.IsLinux())
            devices.Add("Microphone par defaut (PulseAudio)", "default");
        else
            throw new PlatformNotSupportedException("La capture est disponible sous Windows et Linux.");
        return devices;
    }

    /// <summary>
    /// Crée un objet Media configuré pour le périphérique sélectionné.
    /// </summary>
    /// <param name="deviceValue">Le nom de la caméra (Windows) ou le chemin /dev/videoX (Linux)</param>
    public Media CreateCaptureMedia(string deviceValue, string? audioDevice = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceValue);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Configuration pour DirectShow (Windows)
            var media = new Media(_libVLC, "dshow://", FromType.FromLocation);
            media.AddOption($":dshow-vdev={deviceValue}");
            media.AddOption($":dshow-adev={audioDevice ?? "none"}");
            media.AddOption(":live-caching=200");
            media.AddOption(":thread-cout=0");
            media.AddOption(":avcodec-hw=none");
            media.AddOption(":vcodec=h264");

            // Options optionnelles (Exemple : Forcer la résolution ou le framerate)
            // media.AddOption(":dshow-size=1280x720");
            // media.AddOption(":dshow-fps=30");

            return media;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Configuration pour V4L2 (Linux)
            var media = new Media(_libVLC, "v4l2://", FromType.FromLocation);
            media.AddOption($":v4l2-dev={deviceValue}");
            media.AddOption(":live-caching=200");
            media.AddOption(":thread-cout=0");
            media.AddOption(":avcodec-hw=none");
            media.AddOption(":vcodec=h264");
            if (!string.IsNullOrWhiteSpace(audioDevice))
                media.AddOption($":input-slave=pulse://{audioDevice}");

            // Options optionnelles pour Linux
            // media.AddOption(":v4l2-width=1280");
            // media.AddOption(":v4l2-height=720");

            return media;
        }

        throw new PlatformNotSupportedException();
    }
}
