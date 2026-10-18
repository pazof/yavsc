using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using LibVLCSharp.Shared;

// Note : Si vous compilez sous Linux, DirectShowLib n'est pas disponible.
// Pour un projet multiplateforme propre, vous pouvez utiliser des directives de précompilation (#if WINDOWS)
// ou charger la détection DirectShow dans une assembly séparée.
using DirectShowLib;

public class CameraStreamer
{
    private readonly LibVLC _libVLC;

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
                // Pour Windows, VLC utilise le nom de l'appareil
                devices.Add(device.Name, device.Name);
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

    /// <summary>
    /// Crée un objet Media configuré pour le périphérique sélectionné.
    /// </summary>
    /// <param name="deviceValue">Le nom de la caméra (Windows) ou le chemin /dev/videoX (Linux)</param>
    public Media CreateCaptureMedia(string deviceValue)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Configuration pour DirectShow (Windows)
            var media = new Media(_libVLC, "dshow://", FromType.FromLocation);
            media.AddOption($":dshow-vdev={deviceValue}");

            // Options optionnelles (Exemple : Forcer la résolution ou le framerate)
            // media.AddOption(":dshow-size=1280x720");
            // media.AddOption(":dshow-fps=30");

            return media;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Configuration pour V4L2 (Linux)
            var media = new Media(_libVLC, "v4l2://", FromType.FromLocation);
            media.AddOption($":v4l2-vdev={deviceValue}");

            // Options optionnelles pour Linux
            // media.AddOption(":v4l2-width=1280");
            // media.AddOption(":v4l2-height=720");

            return media;
        }

        throw new PlatformNotSupportedException();
    }
}
