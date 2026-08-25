
# Cibles pour installer PostIt.Android en Debug sur l'AVD qemu.
#
# Usage typique :
#   make qemu            # lance l'AVD, attend le boot, build l'APK, l'installe
#   make android-install # (re)build l'APK et l'installe (AVD doit tourner)
#   make android-build   # build l'APK seul (sans install)
#   make qemu-run        # démarre l'AVD en background
#   make qemu-stop       # arrête l'émulateur
#   make qemu-wait-boot  # attend que l'AVD ait fini de booter
#
# Variables surchargeables (make VAR=valeur) :
#   AVD_NAME        default: postit_test_avd
#                    (l'AVD doit être listé par `avdmanager list avd`)
#   ADB_SERIAL      default: emulator-5554
#                    (port standard du premier émulateur lancé)
#   ANDROID_HOME    default: /opt/android-sdk
#                    (le SDK Android local; doit contenir
#                    emulator/emulator et platform-tools/adb)
#   POSTIT_RID      default: android-x64
#                    (doit matcher l'ABI de l'AVD; `avdmanager list avd`
#                    affiche la ligne Tag/ABI)
#   EMU_HEADLESS    default: 0
#                    (1 = lancer l'émulateur sans fenêtre, pour scripter)
#   CONFIG          surcharge la variable CONFIG globale (Debug par
#                    défaut dans ce Makefile). Passer à Release pour
#                    un APK optimisé et signé release.
#   LOGCAT_LINES    default: 200
#                    (nombre de lignes dumpées par `make qemu-logcat`)
#   LOGCAT_FOLLOW   default: 0
#                    (1 = stream live via `make logcat`,
#                    sinon dump one-shot des N dernières lignes)
#   LOGCAT_BOOT_WAIT default: 30
#                    (secondes d'attente entre le clear du buffer,
#                    le `am start`, et le dump final dans
#                    `make qemu-logcat-boot`)
AVD_NAME ?= postit_test_avd
ADB_SERIAL ?= emulator-5554
ANDROID_HOME ?= /opt/android-sdk
POSTIT_RID ?= android-x64
EMU_HEADLESS ?= 0
LOGCAT_LINES ?= 600
LOGCAT_FOLLOW ?= 0
LOGCAT_BOOT_WAIT ?= 30

ANDROID_PACKAGE_NAME = fr.pschneider.postit
POSTIT_ANDROID_CSPROJ := PostIt.Android/PostIt.Android.csproj
POSTIT_APK_DIR := PostIt.Android/bin/$(CONFIG)/net10.0-android/$(POSTIT_RID)
POSTIT_APK := $(POSTIT_APK_DIR)/$(ANDROID_PACKAGE_NAME)-Signed.apk

clean: clean-PostIt clean-PostIt.Android clean-PostIt.Desktop

clean-%:
	rm -rf $*/obj $*/bin

qemu-run:
	@echo "  Starting AVD $(AVD_NAME) on $(ADB_SERIAL)..."
	@mkdir -p /tmp/yavsc-emu
	@EMU_ARGS=""; \
	if [ "$(EMU_HEADLESS)" = "1" ]; then EMU_ARGS="-no-window -no-audio"; fi; \
	$(ANDROID_HOME)/emulator/emulator -avd $(AVD_NAME) $$EMU_ARGS \
	    >/tmp/yavsc-emu/$(AVD_NAME).log 2>&1 & \
	echo "  ✅ Started emulator PID: $$!"

qemu-stop:
	adb -s $(ADB_SERIAL) emu kill
	echo "  ✅ Stopped emulator"

qemu-wait-boot:
	@echo "  Waiting for $(ADB_SERIAL) to finish booting..."
	adb -s $(ADB_SERIAL) wait-for-device
	@for i in $$(seq 1 180); do \
	    BOOTED=$$(adb -s $(ADB_SERIAL) shell getprop sys.boot_completed 2>/dev/null | tr -d '\r\n'); \
	    if [ "$$BOOTED" = "1" ]; then \
	        echo "  ✓ booted in $${i}s"; \
	        exit 0; \
	    fi; \
	    sleep 1; \
	done; \
	echo "  👿  ERROR: device did not boot within 180s." >&2; \
	echo "  Logs: /tmp/yavsc-emu/$(AVD_NAME).log" >&2; \
	exit 1

android-build:
	# EmbedAssembliesIntoApk=true: without this, the Debug APK ships
	# without the managed assemblies in it (they are pushed at runtime
	# via `adb push`, "Fast Deployment"). On the qemu emulator, the
	# runtime cannot find them in `files/.__override__/<rid>/` and
	# aborts at startup with "No assemblies found in '.__override__'"
	# (monodroid-glue.cc:757, SIGABRT). Forcing this property on
	# packages the .dlls into the APK as `assemblies/<rid>/` so the
	# runtime reads them directly.
	#
	# The Xamarin.Android SDK property is `EmbedAssembliesIntoApk`,
	# not `AndroidEnableFastDeployment` (which exists in older
	# templates but is a no-op in the .NET 10 SDK).
	dotnet build $(POSTIT_ANDROID_CSPROJ) \
	    -c $(CONFIG) \
	    -p:RuntimeIdentifier=$(POSTIT_RID) \
	    -p:EmbedAssembliesIntoApk=true \
	    --nologo
	@if [ ! -f "$(POSTIT_APK)" ]; then \
	    echo "  APK not found at $(POSTIT_APK)." >&2; \
	    echo "  Files in $(POSTIT_APK_DIR):" >&2; \
	    ls -la "$(POSTIT_APK_DIR)" 2>/dev/null || echo "  (directory does not exist)" >&2; \
	    exit 1; \
	fi


android-install: android-build
	@echo "  Installing $(POSTIT_APK) on $(ADB_SERIAL)..."
	adb -s $(ADB_SERIAL) install -r "$(POSTIT_APK)" -r
	@echo "  ✅ PostIt.Android installed on $(ADB_SERIAL)"

qemu-uninstall:
	adb -s $(ADB_SERIAL) uninstall $(ANDROID_PACKAGE_NAME)

# Dump recent logcat output for the running PostIt.Android process.
# By default, prints the last $(LOGCAT_LINES) lines (one-shot, with
# `-d`). Set LOGCAT_FOLLOW=1 to follow the stream live instead.
# Filtering is by PID (pidof $(ANDROID_PACKAGE_NAME)), not by tag,
# because Mono/Xamarin can emit logs under several tags
# (mono, PostIt.Android, Avalonia.Android) and tag-based filtering
# would miss the ones not matching. PID-based filtering is exact.
# If the app is not running, pidof returns empty and logcat exits
# silently with no output; that is the expected behaviour for
# "no logs yet".
logcat:
	@PID=$$(adb -s $(ADB_SERIAL) shell pidof $(ANDROID_PACKAGE_NAME) 2>/dev/null | tr -d '\r\n'); \
	if [ -z "$$PID" ]; then \
	    echo "  $(ANDROID_PACKAGE_NAME) is not running on $(ADB_SERIAL)."; \
	    echo "  Start the app first (am start -n $(ANDROID_PACKAGE_NAME)/PostIt.Android.PostItMainActivity)"; \
	    exit 1; \
	fi; \
	echo "  Following PID $$PID (LOGCAT_FOLLOW=$(LOGCAT_FOLLOW), LOGCAT_LINES=$(LOGCAT_LINES))"; \
	if [ "$(LOGCAT_FOLLOW)" = "1" ]; then \
	    adb -s $(ADB_SERIAL) logcat -v time --pid=$$PID $(ANDROID_PACKAGE_NAME); \
	else \
	    adb -s $(ADB_SERIAL) logcat -d -v time -t $(LOGCAT_LINES) --pid=$$PID $(ANDROID_PACKAGE_NAME); \
	fi

# Clear logcat, launch PostIt.Android, then dump everything that was
# emitted during the startup window. Targets the "démarrage KO" case
# where the process starts but Avalonia never renders a frame — the
# logcat trace from process start to first frame is what diagnoses it.
#
# Override LOGCAT_BOOT_WAIT to extend the post-launch wait
# (default 15s; raise to 30+ if the device is slow to boot Avalonia).
LOGCAT_BOOT_WAIT ?= 15


android-start:
	@echo "  Clearing logcat buffer..."
	adb -s $(ADB_SERIAL) logcat -c
	@echo "  Launching $(ANDROID_PACKAGE_NAME)..."
	adb -s $(ADB_SERIAL) shell am start \
	    -n $(ANDROID_PACKAGE_NAME)/PostIt.Android.PostItMainActivity
	@echo "  ✅ $(ANDROID_PACKAGE_NAME) started on $(ADB_SERIAL)"

qemu-logcat-boot: android-start
	@echo "  Waiting $(LOGCAT_BOOT_WAIT)s for the app to start rendering..."
	@sleep $(LOGCAT_BOOT_WAIT)

	@echo "  Dumping logcat (PostIt PID + system buffer):"
	@PID=$$(adb -s $(ADB_SERIAL) shell pidof $(ANDROID_PACKAGE_NAME) 2>/dev/null | tr -d '\r\n'); \
	if [ -n "$$PID" ]; then \
	    echo " ✅ (PID $$PID at dump time)"; \
			sleep 10; \
	    adb -s $(ADB_SERIAL) logcat -d -v time -t $(LOGCAT_LINES) --pid=$$PID; \
	else \
	    echo " 👿 (PostIt process not running at dump time — dumping last $(LOGCAT_LINES) lines unfiltered)"; \
	    adb -s $(ADB_SERIAL) logcat -d -v time -t $(LOGCAT_LINES); \
			exit 1; \
	fi

qemu: qemu-run qemu-wait-boot android-install

.PHONY: clean qemu qemu-run qemu-stop qemu-wait-boot android-build android-install logcat qemu-logcat-boot
