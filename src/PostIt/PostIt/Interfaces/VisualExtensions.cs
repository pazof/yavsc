using System;
using Avalonia;
using Avalonia.Controls;

namespace PostIt.Interfaces;

public static class VisualExtensions
{
    static IntPtr? GetTopLevel(this Visual visual)
    {
        var topLevel = TopLevel.GetTopLevel(visual);
        return topLevel?.TryGetPlatformHandle()?.Handle;
    }
}
