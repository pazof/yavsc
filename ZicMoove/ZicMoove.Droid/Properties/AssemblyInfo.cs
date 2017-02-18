using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Android.App;
using ZicMoove;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ZicMoove.Droid")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ZicMoove.Droid")]
[assembly: AssemblyCopyright("Copyright © Paul Albert Schneider 2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("fr")]
[assembly: ComVisible(false)]
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Add some common permissions, these can be removed if not needed
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]
[assembly: UsesPermission(Android.Manifest.Permission.WakeLock)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessFineLocation)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessCoarseLocation)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessLocationExtraCommands)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessMockLocation)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessNetworkState)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessWifiState)]

[assembly: UsesPermission(Android.Manifest.Permission.AccountManager)]
[assembly: UsesPermission(Android.Manifest.Permission.AuthenticateAccounts)]
[assembly: UsesPermission(Android.Manifest.Permission.GetAccounts)]
[assembly: UsesPermission(Android.Manifest.Permission.Camera)]
[assembly: UsesPermission(Android.Manifest.Permission.CaptureAudioOutput)]
[assembly: UsesPermission(Android.Manifest.Permission.CaptureSecureVideoOutput)]
[assembly: UsesPermission(Android.Manifest.Permission.CaptureVideoOutput)]
[assembly: UsesPermission(Android.Manifest.Permission.ChangeConfiguration)]
[assembly: UsesPermission(Android.Manifest.Permission.ChangeNetworkState)]
[assembly: UsesPermission(Android.Manifest.Permission.ChangeWifiState)]
[assembly: UsesPermission(Android.Manifest.Permission.ChangeWifiMulticastState)]

[assembly: UsesPermission(Android.Manifest.Permission.SignalPersistentProcesses)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadCalendar)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadContacts)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadFrameBuffer)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadInputState)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadFrameBuffer)]

[assembly: UsesPermission(Android.Manifest.Permission.RecordAudio)]
[assembly: UsesPermission(Android.Manifest.Permission.Bluetooth)]
[assembly: UsesPermission(Android.Manifest.Permission.ReceiveBootCompleted)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteCalendar)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteSocialStream)]


[assembly: UsesPermission("com.google.android.c2dm.intent.RECEIVE")]
[assembly: UsesPermission("com.google.android.c2dm.intent.REGISTRATION")]
[assembly: UsesPermission("com.google.android.c2dm.intent.RETRY")]
[assembly: UsesPermission("com.google.android.c2dm.permission.RECEIVE")]
[assembly: UsesPermission("com.google.android.providers.gsf.permission.READ_GSERVICES")]


[assembly: Application( Debuggable = true, Label = Constants.ApplicationLabel, Theme = "@style/MainTheme",
      AllowBackup = true, Icon =  "@drawable/icon" )]
[assembly: MetaData("com.google.android.maps.v2.API_KEY", Value = Constants.MapsV2APIKey)]
[assembly: MetaData("com.google.android.gms.version", Value = "@integer/google_play_services_version")]

[assembly: Permission(Name= Constants.PermissionMapReceive, ProtectionLevel = Android.Content.PM.Protection.Signature)]
[assembly: Permission(Name = Constants.PermissionC2DMessage, ProtectionLevel = Android.Content.PM.Protection.Signature)]

[assembly: UsesPermission(Constants.PermissionMapReceive)]
[assembly: UsesPermission(Constants.PermissionC2DMessage)]

    