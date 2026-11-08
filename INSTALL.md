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

## Resource consumption and CGU

Configure the **same** `ResourceUsage` section on Yavsc.Api, Yavsc.Blogs and
Yavsc.Org. API and Blogs must also use the same application database.
All three hosts and configuration templates inherit the backend defaults
from [ResourceUsageOptions](src/Yavsc.Server/Settings/ResourceUsageOptions.cs).
Deploy matching backend versions across hosts. The public CGU renders the
effective coefficients and limit. The default limit is 1,000 points.
The following is an example explicit override, not a duplicate of defaults.
Use the account's immutable user ID (`sub`), not its login, for overrides:

```json
{
  "ResourceUsage": {
    "ApiCallWeight": 0.2,
    "CpuSecondWeight": 0.1,
    "BandwidthMbWeight": 0.05,
    "StorageMbWeight": 0.5,
    "DefaultLimit": 1000,
    "UserLimits": {
      "example-account-id": 20000
    }
  }
}
```

Environment variables follow ASP.NET's convention, for example
`ResourceUsage__DefaultLimit` and
`ResourceUsage__UserLimits__example-account-id`. Restart all three services
after changing the rule. Invalid weights or limits fail startup.
Existing cumulative `ResourceUsageRecords` are reused, and changing weights
recalculates their gross indicator. Apply the `ResourceSubscriptions`
migration before deploying any of the three hosts: it adds the credit
balance and subscription/payment tables without clearing existing counters.

Consumption is in **points**, independent of `EstimatedCompensation` and
currency. Counters have no automatic reset. Storage means cumulative
instrumented writes, not current occupancy; the historical CPU counter is
instrumented elapsed processing time, not exhaustive CPU profiling.

Authenticated HTTP operations are admitted atomically against known costs
(API call and declared request body). Personal-file uploads additionally
check their aggregate storage cost before writing. Response bytes and
unknown-length request bodies are measured through counting streams, and
remaining costs are recorded after execution, including failed requests.
PostgreSQL transaction-scoped advisory locks serialize admission and
accounting for each user across hosts. In-flight operations can exceed the
limit through costs unknown at admission; subsequent operations return
HTTP 429 with `resource_quota_exceeded` Problem Details. No permanent account
suspension or strict running-operation interruption is performed.

`GET /api/v1/resource-usage/me` returns `Consumption`, `ConsumptionLimit`,
`RemainingConsumption` and `IsQuotaReached`, alongside existing counters.
It also returns `GrossConsumption`, `AppliedCredits` and `CreditBalance`.
`Consumption` is the gross indicator less acquired credits, floored at zero.
`RemainingConsumption` includes both unused free quota and carried credit.
Usage consultation (including the role-protected admin overview), login,
session refresh and the CGU remain accessible without consuming quota.
The quota does not measure local PostIt resources, direct peer calls or
WebSocket traffic. HTTP upload size limits and disk quotas still apply.

The public page `/Home/CGU` describes the effective rule and default limit
(and the individual limit for a signed-in user). It is linked from the Web
footer and PostIt's **About** page, using the configured authentication
authority. PostIt displays consumption and remaining points in **My profile**
and the administration usage views.

### Monthly resource subscriptions (PayPal REST)

The free registration keeps the configured cumulative quota without a
monthly reset. Discovery costs EUR 1 excluding VAT (EUR 1.20 including 20%
VAT) per month; Premium costs EUR 2 excluding VAT (EUR 2.40 including VAT).
Every **confirmed payment** grants up to one / three default quotas,
respectively, with renewal on the UTC monthly anniversary of the PayPal
start date. The pack allowance is captured at subscription creation, not
multiplied by an individual user's quota override.

At reconciliation, old carried credit is used first; the new allowance
erases remaining net usage. Two thirds of the unused allowance are added
to permanent carried credit, rounded down to eight decimal places.
Old carried credit never loses another third and survives cancellation.
Example: 400 points of usage and 1,000 new points erase 400 and carry
400; an idle Premium month carries 2,000 points.
Counters are never deleted. Pending/failed payments give no credit.
Refunds (including partial refunds) revoke the corresponding award once.

Discovery grants **twice `Constants.DefaultFSQ`** (1,000 MiB), and Premium
**five times `Constants.DefaultFSQ`** (2,500 MiB) of disk space during each
confirmed, non-reversed paid monthly period. Free registrations use the standard quota.
The reference is the standard free quota (500 MiB), not an individual's
administratively adjusted `DiskQuota`. That base quota is never overwritten.
New user objects now use the existing 500 MiB database default rather than
the former 512 MiB CLR initializer; existing accounts retain their stored quota.
Cancellation retains the disk allowance through the UTC anniversary ending
the paid period; expiry or refund restores the base quota without deleting
existing files. Uploads exceeding available space are refused before writing.
The maximum size of one file and separate podcast limits are unchanged.
Pending/failed payments grant no disk bonus. Apply the
`ResourceSubscriptions`, `PremiumDiskQuota` and `DiscoveryDiskQuota` EF migrations to the shared
database before deploying any host. Profile and usage responses expose the
effective disk quota; usage also exposes `PremiumDiskQuotaUntil` and `DiscoveryDiskQuotaUntil`.
When paid periods overlap, Premium takes precedence until its expiry, then
any still-paid Discovery period applies.

Configure only **Yavsc.Org** for PayPal subscriptions. API and Blogs need
the migrated shared database and matching resource weights, not PayPal
credentials. Existing Classic donation settings remain unchanged.
Use the existing `PayPal:Mode`, `PayPal:ClientId` and `PayPal:ClientSecret`
REST settings; keep credentials in user-secrets or environment variables.

```json
{
  "ResourceSubscriptions": {
    "DiscoveryPlanId": "P-REPLACEWITHDISCOVERYPLAN",
    "PremiumPlanId": "P-REPLACEWITHPREMIUMPLAN",
    "WebhookId": "REPLACEWITHWEBHOOKID",
    "DiscoveryPriceExcludingTax": 1,
    "PremiumPriceExcludingTax": 2,
    "VatPercent": 20
  }
}
```

Before enabling checkout, create a PayPal subscription product and two
**ACTIVE** billing plans in the same REST app and environment:
one indefinite `REGULAR` cycle with `interval_unit: MONTH`,
`interval_count: 1`, `total_cycles: 0`, and EUR `fixed_price.value`
of **1.20** / **2.40** respectively. These are **already VAT-inclusive**
amounts: do not add PayPal tax, setup fees, trials, quantity support or
shipping fees. Disable billing outstanding balances in the plans
(`auto_bill_outstanding: false`) to avoid bundling missed installments
into an amount that does not match one monthly payment.
Checkout checks the configured plan against its expected TTC amount and
refuses automatic collection of outstanding balances.
Numerically invalid pricing fails startup; missing plan IDs/webhook/
credentials returns an explicit unavailable error instead of a fake success.

Register the HTTPS endpoint
`https://<Yavsc.Org host>/ResourceSubscription/Notify` in that REST app.
Subscribe to `BILLING.SUBSCRIPTION.ACTIVATED`, `UPDATED`, `CANCELLED`,
`SUSPENDED`, `EXPIRED`, `PAYMENT.FAILED`, and `PAYMENT.SALE.COMPLETED`,
`REFUNDED`, `REVERSED` (the lifecycle events use the
`BILLING.SUBSCRIPTION.` prefix). Set the returned webhook ID in configuration.
The server verifies the signature through PayPal, then fetches the bound
subscription and transaction history; it never grants credit from an
unverified webhook or the browser return. Payment IDs and monthly cycles
are unique, and the same per-user PostgreSQL lock serializes usage and awards.
Delayed notifications take effect on the usage measured when reconciled.

Users manage subscriptions at `/ResourceSubscription`, linked from the Web
footer and PostIt's About/profile pages. A browser Web login may be required
independently of PostIt's session. Create/refresh/cancel require antiforgery
tokens and ownership; the management page remains available at the quota.
**Refresh payments** reconciles missed notifications and historical refunds.
Cancellation is confirmed with PayPal, not merely recorded locally.
Only one non-terminal subscription is allowed per account.
An uncertain creation is retried with its same PayPal request ID for at
most 24 hours; an older unresolved creation requires merchant-side review
before allowing a second checkout. Do not delete that record and retry
without checking PayPal for a possible existing agreement.

Run a real Sandbox approval, first payment, renewal, repeated webhook,
refund and cancellation before enabling production. The repository tests
use synthetic PayPal transports and do not validate merchant credentials
or PayPal account eligibility.

## Podcast live broadcasts and replays

PostIt Desktop's home page provides **Podcasts : directs et replays**.
Install LibVLC 3 with the `livehttp`, MPEG-TS, x264 and AAC plugins on the
Desktop machine. Windows capture uses DirectShow; Linux uses V4L2 and the
default PulseAudio microphone. FFmpeg is **not** a client dependency.
Select a camera and optionally a microphone, enter a title, then start.
An **Apercu local de la capture** appears at the top of the Podcasts page on
Android and Desktop. It uses the current capture, not a second camera session
or the server replay. Android displays the same oriented frames as the encoder,
with black bars to preserve proportions; Desktop duplicates LibVLC's video
output. Local microphone playback is disabled to avoid acoustic feedback.
Stopping releases the preview; hiding/recreating the Android preview surface
does not restart the encoder or change its timestamps.
Stopping publishes the retained replay; navigating away or logging out
stops capture too. A second signed-in PostIt instance can watch the live
broadcast while it is being captured.

Both upload and playback require a bearer token with the `api` scope.
All authenticated API users can watch the catalogue, live broadcasts and
replays; only the broadcaster can upload, complete or delete their podcast.
Media is not anonymous and no access token is included in a playback URL.

Configure Yavsc.Api's `Podcast` section (or environment variables such as
`Podcast__RootPath`). No database migration is needed. Install **FFmpeg 5+
on the API server** (with `concatf`, MPEG-TS, MP4 and AAC bitstream-filter
support; for Debian/Ubuntu: `apt-get install ffmpeg`). The API runtime stage
in the main Dockerfile includes it. Only stream-copy remuxing is performed,
not video/audio re-encoding:

```json
{
  "Podcast": {
    "RootPath": "/srv/yavsc-private/podcasts",
    "MaxSegmentBytes": 8388608,
    "MaxOwnerBytes": 1073741824,
    "MaxSegments": 7200,
    "MaxOwnerPodcasts": 100,
    "IdleTimeoutSeconds": 60,
    "FfmpegPath": "ffmpeg",
    "ReplayTimeoutSeconds": 600
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
and forward Authorization, Range and Content-Range on every API request/response.
The retained MPEG-TS **and** the additional MP4 count toward `MaxOwnerBytes`.
Allow enough free quota and disk space for both copies and the temporary MP4;
deletion removes all copies.

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

On completion, the API queues a private, seekable **MP4** (`-c copy`,
`+faststart`). The persisted `replay.state` moves through `Pending`,
`Processing`, `Ready` or `Failed`; completion itself does not wait for FFmpeg.
The single API worker also queues older Completed indexes automatically,
recovers interrupted preparation after a restart, and scans every five seconds.
Interrupted broadcasts keep MPEG-TS playback unless the owner explicitly
completes them. Source segments and existing HLS routes remain available.
Preparation failure keeps all source media and reports an error; after checking
server logs, FFmpeg installation, timeout and quota, the owner can use PostIt's
**Relancer la preparation MP4** action, or `POST /api/v1/podcasts/{id}/replay/retry`.
The MP4 is exposed only when ready, at authenticated
`GET /api/v1/podcasts/{id}/replay.mp4` (also HEAD; byte ranges supported).

PostIt's Android and Desktop player waits for MP4 preparation, then fetches
bounded authenticated ranges with token refresh rather than giving LibVLC
a bearer URL. Replay controls overlay the native video: play/pause, position,
duration and draggable seek bar. They hide after three seconds without
interaction while playing, stay visible during seeking/pause/keyboard focus,
and reappear on pointer movement or touch.
The live player still joins up to three segments behind the edge. Native
camera/microphone permissions, drivers and codecs must still be verified
on each target Windows/Linux machine.

### Android podcasts

The Android APK bundles LibVLC and uses a native Android video surface inside
the same Avalonia video page as desktop. Catalogue, playback controls and
authenticated segment downloads are shared; no external browser, public media
URL or camera/microphone permissions are needed to watch a podcast.

Video playback keeps the screen awake while PostIt is visible. Pausing,
stopping, reaching the end, a playback error or leaving the video releases
this request; backgrounding the app always allows the screen to sleep.

The podcast page offers **Garder l'ecran allume pendant l'apercu camera et le
direct** on Android. This preference is saved and disabled by default. It
only keeps the screen awake during an active camera preview or broadcast
while PostIt is visible, and can be changed during capture. Turning it off
does not stop capture or alter the existing background broadcast service.

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
