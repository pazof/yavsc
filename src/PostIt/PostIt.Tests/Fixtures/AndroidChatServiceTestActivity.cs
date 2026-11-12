#if ANDROID
using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Android;
using PostIt.Android.Services;
using PostIt.Services;
using PostIt.ViewModels;

namespace PostIt.Tests;

/// <summary>Debug-only boot composition test, without Avalonia or network startup.</summary>
[Activity(Name = "fr.pschneider.postit.ChatServiceTestActivity", Exported = true)]
public sealed class AndroidChatServiceTestActivity : Activity
{
    protected override async void OnCreate(Bundle? state)
    {
        base.OnCreate(state);
        var resultPath = Path.Combine(FilesDir!.AbsolutePath, "chat-service-test-result.txt");
        try
        {
            await PostIt.Android.Tests.ChatServiceEpochTests.VerifyAsync();
            var boot = AndroidServiceProvider.GetOrCreate();
            PlatformBootstrap.InitPlatform();
            var ui = Platform.CreateServiceProvider!.Invoke();
            Require(ReferenceEquals(boot, ui), "boot and UI providers differ");
            Require(ReferenceEquals(boot.GetRequiredService<YavscApiClient>(),
                ui.GetRequiredService<YavscApiClient>()), "API client differs");
            Require(ReferenceEquals(boot.GetRequiredService<ChatHubConnection>(),
                ui.GetRequiredService<ChatHubConnection>()), "chat transport differs");
            Require(ReferenceEquals(boot.GetRequiredService<Settings>(),
                ui.GetRequiredService<YavscApiClient>().Settings), "settings differ");
            Require(ReferenceEquals(Platform.BackgroundChatService, AndroidBackgroundChatService.Instance),
                "native adapter missing");
            using (var battery = AndroidBatteryOptimization.CreateIntent(this))
            {
                var power = (PowerManager)GetSystemService(PowerService)!;
                var exempt = power.IsIgnoringBatteryOptimizations(PackageName!);
                Require(exempt ? battery is null
                        : battery?.Action == global::Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations,
                    "battery exemption must not prompt when already granted");
                Require(battery?.DataString == (exempt ? null : "package:" + PackageName),
                    "battery settings package mismatch");
                Require(CheckSelfPermission("android.permission.REQUEST_IGNORE_BATTERY_OPTIMIZATIONS")
                    == Permission.Granted, "battery exemption request permission missing");
            }
            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                var name = new ComponentName(this, Java.Lang.Class.FromType(typeof(ChatConnectionService)));
                var service = PackageManager!.GetServiceInfo(name, PackageInfoFlags.MetaData)!;
                Require(service.ForegroundServiceType == (ForegroundService)0,
                    "chat must not be a foreground service");
                Require(!service.Exported, "chat service must not be exported");
            }
            File.WriteAllText(resultPath, "PASS\nepoch-races=passed\nprovider=shared\napi=shared\nsettings=shared\ntransport=shared\nAvalonia=not-initialized");
        }
        catch (Exception ex) { File.WriteAllText(resultPath, $"FAIL\n{ex}"); }
        finally { Finish(); }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
#endif
