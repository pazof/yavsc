using Avalonia.Headless.XUnit;
using PostIt.Services;
using PostIt.ViewModels;
using Yavsc.Api.Client;
using Yavsc.Abstract.Identity;
using Yavsc.Abstract.Streaming;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
public sealed class PodcastPreferencesTests
{
    [AvaloniaFact]
    public async Task Detection_selection_and_title_survive_settings_reload_without_detecting_again()
    {
        var path = Path.Combine(Path.GetTempPath(), $"postit-preferences-{Guid.NewGuid():N}.json");
        var previous = Platform.GetPodcastCaptureDevices;
        try
        {
            Platform.GetPodcastCaptureDevices = () => new(
                [new("Front", "front"), new("Rear", "rear")],
                [new("None", ""), new("USB", "usb")]);
            var settings = new Settings(path);
            await using var api = new YavscApiClient(settings, new TokenStore(path + ".tokens"));
            var session = new SessionStatusViewModel();
            var vm = new PodcastsPageViewModel(new PodcastApiClient(api, () => settings.ApiUrl), api, session);
            vm.DetectDevicesCommand.Execute(null);
            vm.SelectedCamera = vm.Cameras[1];
            vm.SelectedMicrophone = vm.Microphones[0];
            vm.Title = "Mon emission habituelle";
            Assert.False(vm.KeepScreenOnDuringCapture);
            vm.KeepScreenOnDuringCapture = true;
            Assert.False(vm.BlurPodcastBackground);
            vm.BlurPodcastBackground = true;
            vm.DetectDevicesCommand.Execute(null);
            Assert.Equal("rear", vm.SelectedCamera?.Value);
            Assert.Equal("", vm.SelectedMicrophone?.Value);

            var restored = new Settings(path);
            restored.Authentication.Authority = "";
            restored.Load();
            Assert.False(restored.IsDirty);
            Assert.Equal("Mon emission habituelle", restored.PodcastDefaultTitle);
            Assert.Equal("rear", restored.PodcastCameraId);
            Assert.Equal("", restored.PodcastMicrophoneId);
            Assert.True(restored.PodcastKeepScreenOn);
            Assert.True(restored.PodcastBlurBackground);
            Platform.GetPodcastCaptureDevices = () => throw new InvalidOperationException("Must use saved devices.");
            await using var otherApi = new PreferencesApi(restored, new TokenStore(path + ".tokens"));
            var other = new PodcastsPageViewModel(new PodcastApiClient(otherApi, () => restored.ApiUrl), otherApi, session);
            Assert.Equal(2, other.Cameras.Count);
            Assert.Equal(2, other.Microphones.Count);
            Assert.Equal("rear", other.SelectedCamera?.Value);
            Assert.Equal("", other.SelectedMicrophone?.Value);
            Assert.Equal(vm.Title, other.Title);
            Assert.True(other.KeepScreenOnDuringCapture);
            Assert.True(other.BlurPodcastBackground);
            restored.PodcastCameraId = "front";
            restored.PodcastMicrophoneId = "usb";
            restored.PodcastKeepScreenOn = false;
            restored.PodcastBlurBackground = false;
            await other.ActivateAsync();
            Assert.Equal("front", other.SelectedCamera?.Value);
            Assert.Equal("usb", other.SelectedMicrophone?.Value);
            Assert.False(other.KeepScreenOnDuringCapture);
            Assert.False(other.BlurPodcastBackground);

            Platform.GetPodcastCaptureDevices = () => new([], []);
            other.DetectDevicesCommand.Execute(null);
            Assert.Null(other.SelectedCamera);
            Assert.Null(other.SelectedMicrophone);
            Assert.Contains("Aucune camera", other.Status);
        }
        finally
        {
            Platform.GetPodcastCaptureDevices = previous;
            File.Delete(path);
            File.Delete(path + ".tokens");
        }

    }

    private sealed class PreferencesApi(Settings settings, TokenStore store) : YavscApiClient(settings, store)
    {
        public override Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            object response = path.EndsWith("/account/me", StringComparison.Ordinal)
                ? new Me { UserId = "owner" } : new List<PodcastResponse>();
            return Task.FromResult((T)response);
        }
    }

    [AvaloniaFact]
    public async Task Failed_save_is_reported_without_losing_the_title()
    {
        var path = Path.Combine(Path.GetTempPath(), $"postit-preferences-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        try
        {
            var settings = new Settings(path);
            await using var api = new YavscApiClient(settings, new TokenStore(path + ".tokens"));
            var vm = new PodcastsPageViewModel(new PodcastApiClient(api, () => settings.ApiUrl), api, new());
            vm.Title = "Titre conserve";
            Assert.Equal("Titre conserve", vm.Title);
            Assert.Contains("sauvegarde impossible", vm.Status);
            Assert.True(settings.IsDirty);
        }
        finally { Directory.Delete(path); File.Delete(path + ".tokens"); }
    }
}
