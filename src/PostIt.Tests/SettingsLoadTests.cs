using System;
using System.IO;
using Xunit;

namespace PostIt.Tests;

public class SettingsLoadTests
{
    /// <summary>
    /// On the dev machine, the user-level settings file
    /// (~/.config/PostIt/postit-settings.json) does not exist, so Load()
    /// must fall back to the embedded default resource shipped inside
    /// PostIt.dll.
    /// </summary>
    [Fact]
    public void Load_falls_back_to_embedded_resource_when_user_file_missing()
    {
        // Skip if a user-level file exists (CI / different dev machines).
        var userConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PostIt",
            "postit-settings.json");
        if (File.Exists(userConfigPath))
        {
            return; // nothing to assert: user file wins.
        }

        var settings = new PostIt.Settings();
        settings.Load();

        // The bundled postit-settings.json points at yavsc.pschneider.fr.
        Assert.False(string.IsNullOrWhiteSpace(settings.Authentication?.Authority));
        Assert.Equal("postit", settings.Authentication.ClientId);
    }
}