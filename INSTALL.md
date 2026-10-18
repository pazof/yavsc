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
