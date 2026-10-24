# Install

With internet access, Git, dotnet, Node.js, and GNU/Makefile, simply follow these instructions, in order to have the server part installed:

```bash
git clone https://github.com/pschneider/yavsc.git
cd yavsc
dotnet restore
npm install          # restore front-end dependencies
npm run build:js     # build the JS/CSS bundles into wwwroot
cd contrib
make install
```

The front-end bundles (under `src/Yavsc.Org/wwwroot/js/*.bundle.min.js`
and `src/Yavsc.Org/wwwroot/css/*.min.css`) are produced by `npm run
build:js` and committed to the repo, so the runtime .NET stack does
not need a Node toolchain at publish time. Re-run `npm run build:js`
after editing any of the entry files under `src/Yavsc.Org/wwwroot/js/`
or any of the `build/*.entry.js` bundle manifests.

About the mobile app,

```bash
cd src/PostIt/PostIt.Android
dotnet publish -c Release
```

and then, install the app, from the `src/PostIt/PostIt.Android/bin/Release/net10.0/PostIt.apk`


## PostIt.Desktop on Windows (per-user installer)

Build prerequisites: the .NET 10 SDK and NSIS 3 (with its standard
Modern UI 2 includes). From the repository root, publish a self-contained
Windows x64 application, then package the **entire** publish directory:

```powershell
dotnet publish src/PostIt/PostIt.Desktop/PostIt.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false
makensis /DAPP_VERSION=1.1.0-estimate.1 contrib/Windows/postit.nsi
```

Set `APP_VERSION` to the version of the published application (the
`Version` property in `PostIt.Desktop.csproj`). NSIS requires this value;
it is used in the installer filename and Windows' installed-apps list.
On Linux, use `makensis -DAPP_VERSION=1.1.0-estimate.1 contrib/Windows/postit.nsi`
instead. Cross-publishing uses the same `dotnet publish` command.
If `makensis` is not on `PATH` on Windows, invoke the executable from
the NSIS installation directory.

The result is `contrib/Windows/PostIt-<version>-win-x64-Setup.exe`.
NSIS also accepts `PUBLISH_DIR` (an absolute path to a different
**win-x64** publish directory) and `OUTPUT_FILE` (the output executable
path), with the same `/DNAME=value` or `-DNAME=value` syntax. Quote an
entire define argument when its value contains spaces.

The installer does not request elevation or install a shared .NET runtime.
It installs in `%LOCALAPPDATA%\Yavsc\PostIt.Desktop`, creates a Start menu
shortcut for the current user and registers `postit://` in
`HKCU\Software\Classes` so browser-based sign-in returns to PostIt.
Windows' installed-apps list provides the uninstaller. Close PostIt before
installing an update or uninstalling it.

The `app` subdirectory is reserved for published application files and is
removed recursively on uninstall. User settings and tokens stored outside
this directory are preserved. Reinstalling uses the recorded installation
directory and overwrites the packaged files.

The self-contained publication includes .NET, but does **not** add native
LibVLC libraries: video playback still requires a compatible Windows x64
LibVLC installation. PostIt reports an unavailable video player and offers
the system browser as an alternative.

Before distributing a release, test installation, startup, the `postit://`
login callback, updating and uninstallation on Windows with a standard
(non-administrator) account. Installer compilation alone does not validate
these Windows runtime behaviours.

## Podcast live broadcasts and replays

PostIt Desktop's home page provides **Podcasts : directs et replays**.
Install LibVLC 3 with the `livehttp`, MPEG-TS, x264 and AAC plugins on the
Desktop machine. Windows capture uses DirectShow; Linux uses V4L2 and the
default PulseAudio microphone. FFmpeg is **not** a runtime dependency.
Select a camera and optionally a microphone, enter a title, then start.
Stopping publishes the retained replay; navigating away or logging out
stops capture too. A second signed-in PostIt instance can watch the live
broadcast while it is being captured.

Both upload and playback require a bearer token with the `api` scope.
All authenticated API users can watch the catalogue, live broadcasts and
replays; only the broadcaster can upload, complete or delete their podcast.
Media is not anonymous and no access token is included in a playback URL.

Configure Yavsc.Api's `Podcast` section (or environment variables such as
`Podcast__RootPath`). No database migration or server-side encoder is needed:

```json
{
  "Podcast": {
    "RootPath": "/srv/yavsc-private/podcasts",
    "MaxSegmentBytes": 8388608,
    "MaxOwnerBytes": 1073741824,
    "MaxSegments": 7200,
    "MaxOwnerPodcasts": 100,
    "IdleTimeoutSeconds": 60
  }
}
```

`RootPath` must be absolute, writable by Yavsc.Api and **outside any static
file directory**. The default is `DataDir/podcasts` under the API application
base directory. Use a persistent volume and back up this entire directory
(indexes and segments together). This implementation supports **one API
process per storage root**, not multiple writers or replicas.

Defaults allow 8 MiB per segment (also the HTTP endpoint's hard limit),
1 GiB retained per owner, 7,200 segments per episode and 100 episodes per
owner. At the target two-second segment duration, this is roughly four hours
per episode; the byte quota can be reached earlier. Podcast quotas are
separate from the legacy user-files `DiskQuota`. Deleting an ended podcast
releases its podcast quota. Reverse proxies must allow an 8 MiB PUT body
and forward Authorization on every API request.

A broadcast without a new committed segment for 60 seconds, or present
when the API restarts, becomes an interrupted replay on the next catalogue,
manifest or upload request. Already committed media remains playable.
The broadcaster retries transient upload errors twice using the same
sequence and content; it stops if capture fails or the pending local media
exceeds 64 MiB. Unacknowledged media is retained under
`%LOCALAPPDATA%\PostIt\podcast-capture` (Linux: the corresponding local
application data directory), with `podcast.json` and the capture playlist
for manual recovery. There is no automatic resumption after an application
restart; these private local recordings may be removed manually when no
longer needed.

The replay is **HLS/MPEG-TS**, not an MP4 download. PostIt's authenticated
player joins a live broadcast up to three segments behind the edge and
reads a replay from its beginning; seeking is not implemented. Native
camera/microphone permissions, drivers and codecs must still be verified
on each target Windows/Linux machine.

### Android podcasts

The Android APK bundles LibVLC and uses a native Android video surface inside
the same Avalonia video page as desktop. Catalogue, playback controls and
authenticated segment downloads are shared; no external browser, public media
URL or camera/microphone permissions are needed to watch a podcast.

Leaving the app stops playback and cancels downloads. Returning recreates the
player: a replay starts from the beginning, and a live stream rejoins near the
live edge. Seeking and background audio are not supported.

Broadcasting requires **Android 8 (API 26) or newer**. In the shared podcast
page, detect devices, select front/rear camera and optionally the microphone,
then start the live broadcast while PostIt is visible. Camera permission,
microphone permission when selected, and notification permission on Android 13+
must be granted. A permission denial is reported in the page without starting
capture or creating a server broadcast. Permissions are requested before the
server session is created so a long permission dialog cannot expire it.
Watching does not request these permissions.

Android uses Camera2 and its native MediaRecorder H.264/AAC encoder, up to 480p at 30 fps,
not the desktop LibVLC camera inputs or software encoders. MPEG-TS is segmented
at IDR frames, with timestamps used for segment durations. Closed segments
enter the same ordered upload/retry/quota/replay pipeline as desktop.
Devices without a supported camera/encoder report a startup error.

Camera frames are rotated through an OpenGL surface before encoding, so both
live playback and replays have upright pixels without relying on rotation
metadata (not supported by the MPEG-TS recorder). The sensor, front/rear lens
and display orientation are sampled when capture starts; portrait output swaps
width and height. Camera2 may already include sensor rotation in the
SurfaceTexture transform; only the remaining rotation is applied by OpenGL.
Keep that device orientation during a broadcast, or stop and
start a new broadcast after turning the device.

A camera/microphone foreground service and a partial wake lock maintain the
broadcast when PostIt is backgrounded or the screen locks. Its ongoing
notification returns to the app and offers **Arreter et publier**. Stopping
flushes the final segment before completing the replay. Leaving the podcast
page or logging out still stops capture. Android's explicit service stop,
process termination or application restart does not automatically restart a
camera; segments already received by the server remain available under the
interrupted-replay rules above. Device power policies can impose additional
restrictions and must be tested on the intended physical devices.

**Native library compatibility:** the Android app now uses
`VideoLAN.LibVLC.Android` 3.7.0-beta, which is the minimum compatible package
for 16 KiB-page devices. Older 3.6.5 binaries can fail at runtime with
`failed to load libc++_shared.so` or the linker error `XA0141` because the
native libraries are not aligned for 16 KiB memory pages. A successful APK
build or a test on a 4 KiB-page emulator does not establish compatibility
for 16 KiB-page devices or Play distribution requiring that alignment.

Build with the Android workload and SDK configured:

```bash
dotnet build src/PostIt/PostIt.Android/PostIt.Android.csproj
```

On an Android device, verify replay video/audio, live playback, pause/resume,
rotation, Back navigation and returning after backgrounding the app. Also
verify an expired session or interrupted network reports a playback error.
For broadcasting, test front/rear cameras, microphone enabled/disabled,
permission denial, screen lock, background capture, notification stop and the
final replay. No automatic retry of a denied permission or a killed camera
service is performed.

The Debug APK includes a local native-capture test activity (excluded from
Release); it does not create a server podcast. On a dedicated emulator, grant
camera, audio and notification permissions, then run:

```bash
adb shell am start -W -f 0x18000000 -n fr.pschneider.postit/fr.pschneider.postit.PodcastCaptureTestActivity
# Wait about 20 seconds for foreground/background recording and final flush.
adb shell run-as fr.pschneider.postit cat files/podcast-capture-test-result.txt
# Repeat with the second camera, no audio, and the notification stop action:
adb shell am start -W -f 0x18000000 -n fr.pschneider.postit/fr.pschneider.postit.PodcastCaptureTestActivity --es microphone false --es camera front --es notificationStop true
```

The result reports the segment count, the background progress and the private
recording directory under `files/podcast-capture-tests`. Recordings remain
there for local decoder inspection and can be removed after the test.

The implementation was checked on an Android 16 x64 emulator with 4 KiB
pages: camera/microphone capture produced five independently decodable
H.264/AAC segments across foreground/background operation; the second camera
without audio produced six decodable H.264 segments, including screen-off
operation and stop through the notification action. The foreground service
was gone after both tests. These local tests do not replace an authenticated
Android-to-server broadcast/replay test or validation on physical devices.

## Resources

System resources needed to install.

### Some TCP/IP ports to listen

The following provided services need all a port to listen :

* Org
* Blogs
* Api (to become obsolete, in favor of Performer + Client + Com + Moderation )

### A Software bloc name

In this doc, let's say, `Yavsc`

### A domain name

Wait ... `pschneider.fr`
You'll have to see with your provider, in order to power it, and make it point to your host ip, at least concerning the ports 80 & 443.

### A Postgresql db

The database must be created, and we need its connection string, allowed to modify the data dictionary, see the `.env` file

### An Smtp mail service

You'll need to send e-mail ... using smtp and the provider of your choice, see the `.env` file.

### Google

I'll have to make with it, a least a moment, it needs a *service account*, and some setup, 
in the `appsettings.*.json` file, and 
the execution environment variable GOOGLE_APPLICATION_CREDENTIALS to a value pointing the json description file for the Google Service account.
