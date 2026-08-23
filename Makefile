
CONFIG=Debug
FRAMEWORK=net9.0
DESTDIR=/tmp/yavsc
APP_PATH=srv/www/yavsc
APP_FULL_PATH=$(DESTDIR)/$(APP_PATH)

include .env

all:
	dotnet build --nologo

clean:
	dotnet clean -c $(CONFIG)

src/Yavsc/bin/output/wwwroot:
	dotnet --project src/Yavsc.Org/Yavsc.Org.csproj publish

test:
	ASPNETCORE_ENVIRONMENT=Development dotnet test

watch:
	dotnet watch -p:Configuration=$(CONFIG) --project src/Yavsc/Yavsc.csproj

src/Yavsc.Abstract/bin/$(CONFIG)/$(FRAMEWORK)/Yavsc.Abstract.dll:
	dotnet build -p:Configuration=$(CONFIG) --project src/Yavsc.Abstract/Yavsc.Abstract.csproj

src/Yavsc.Server/bin/$(CONFIG)/$(FRAMEWORK)/Yavsc.Server.dll:
	dotnet build -p:Configuration=$(CONFIG) --project src/Yavsc.Server/Yavsc.Server.csproj

src/Yavsc/bin/$(CONFIG)/$(FRAMEWORK)/Yavsc.dll:
	dotnet build -p:Configuration=$(CONFIG) --project src/Yavsc.Org/Yavsc.Org.csproj

$(DESTDIR):
	mkdir $(DESTDIR)

install: $(DESTDIR)
	dotnet publish src/Yavsc.Org/Yavsc.Org.csproj -c Release -o $(APP_FULL_PATH)
	dotnet publish src/Api/Api.csproj -c Release -o $(APP_FULL_PATH)
	sudo chown -R www-data:www-data $(APP_FULL_PATH)

docker-image:
	docker build .

docker-build:
	docker compose up --build

docker-run:
	docker run -d -p 5000:5000 --name yavsc yavsc

# Crée une branche release/<V> depuis main, met à jour les
# `<Version>` des .csproj via dotnet-gitversion, et la
# pousse sur origin.
#
# Usage : make release V=1.0.7-rc1
#
# Pré-requis : être sur main, working tree clean. La cible
# vérifie les deux et refuse sinon — elle ne fait JAMAIS
# de checkout automatique, c'est à l'opérateur de s'être
# positionné sur la bonne branche au préalable (sinon le
# bump pourrait partir sur une branche tierce par accident).
#
# Notes :
# - Le nom de branche vient de l'argument V (ex: 1.0.7-rc1
#   donne release/1.0.7-rc1). C'est une étiquette d'intention,
#   pas la version assembly.
# - La version dans les .csproj vient de GitVersion qui la
#   calcule depuis l'historique git (tag le plus proche +
#   nombre de commits). C'est la version assembly réelle.
# - L'ordre (fetch → branche → bump → push) garantit qu'on
#   part d'un main synchro et qu'on ne pollue pas main avec
#   le bump (qui vit sur la branche release).
# - Fail-fast si la branche existe déjà en local ou sur origin.
release:
	@if [ -z "$(V)" ]; then \
		echo "Usage: make release V=<version>"; \
		echo "  V : version semver (ex. 1.0.7-rc1) — sert à nommer la branche."; \
		exit 1; \
	fi
	@CURRENT=$$(git branch --show-current); \
	if [ "$$CURRENT" != "main" ]; then \
		echo "Refus : la cible doit être lancée depuis main."; \
		echo "  Branche courante : $$CURRENT"; \
		echo "  Fais : git checkout main && git pull --ff-only origin main"; \
		exit 1; \
	fi
	@if [ -n "$$(git status --porcelain)" ]; then \
		echo "Working tree sale, refus de créer une branche release."; \
		git status --short; \
		exit 1; \
	fi
	@BRANCH="release/$(V)"; \
	if git show-ref --verify --quiet "refs/heads/$$BRANCH"; then \
		echo "La branche $$BRANCH existe déjà en local."; \
		echo "  Pour la supprimer : git branch -D $$BRANCH"; \
		exit 1; \
	fi; \
	if git ls-remote --exit-code --heads origin "$$BRANCH" >/dev/null 2>&1; then \
		echo "La branche $$BRANCH existe déjà sur origin."; \
		exit 1; \
	fi; \
	echo "==> Fetch + vérification synchro main"; \
	git fetch origin main; \
	if ! git merge-base --is-ancestor origin/main HEAD; then \
		echo "main a avancé plus loin que HEAD. Fais :"; \
		echo "  git pull --ff-only origin main"; \
		exit 1; \
	fi; \
	echo "==> Création de $$BRANCH depuis main"; \
	git checkout -b "$$BRANCH"; \
	echo "==> dotnet-gitversion /updateprojectfiles"; \
	dotnet-gitversion /updateprojectfiles; \
	echo "==> Commit du bump"; \
	git add .; \
	if git diff --cached --quiet; then \
		echo "Pas de changements à committer (gitversion n'a produit aucune diff)."; \
	else \
		git commit -m "chore(release): bump version via gitversion for $(V)"; \
	fi; \
	echo "==> Push de $$BRANCH sur origin"; \
	git push -u origin "$$BRANCH"; \
	echo "==> Terminé. Branche $$BRANCH live sur origin."

# Cibles pour installer PostIt.Android en Debug sur l'AVD qemu.
#
# Usage typique :
#   make qemu           # lance l'AVD, attend le boot, build l'APK, l'installe
#   make qemu-install   # (re)build l'APK et l'installe (AVD doit tourner)
#   make qemu-build     # build l'APK seul (sans install)
#   make qemu-run       # démarre l'AVD en background
#   make qemu-stop      # arrête l'émulateur
#   make qemu-wait-boot # attend que l'AVD ait fini de booter
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
#                    (1 = stream live via `make qemu-logcat`,
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
LOGCAT_LINES ?= 200
LOGCAT_FOLLOW ?= 0
LOGCAT_BOOT_WAIT ?= 15

POSTIT_ANDROID_CSPROJ := src/PostIt/PostIt.Android/PostIt.Android.csproj
POSTIT_APK_DIR := src/PostIt/PostIt.Android/bin/$(CONFIG)/net10.0-android/$(POSTIT_RID)
POSTIT_APK := $(POSTIT_APK_DIR)/fr.pschneider.PostIt-Signed.apk

qemu-run:
	@echo "  Starting AVD $(AVD_NAME) on $(ADB_SERIAL)..."
	@mkdir -p /tmp/yavsc-emu
	@EMU_ARGS=""; \
	if [ "$(EMU_HEADLESS)" = "1" ]; then EMU_ARGS="-no-window -no-audio"; fi; \
	$(ANDROID_HOME)/emulator/emulator -avd $(AVD_NAME) $$EMU_ARGS \
	    >/tmp/yavsc-emu/$(AVD_NAME).log 2>&1 & \
	echo "  emulator PID: $$!"

qemu-stop:
	adb -s $(ADB_SERIAL) emu kill

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
	echo "  ERROR: device did not boot within 180s." >&2; \
	echo "  Logs: /tmp/yavsc-emu/$(AVD_NAME).log" >&2; \
	exit 1

qemu-build:
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


qemu-install: qemu-build
	@echo "  Installing $(POSTIT_APK) on $(ADB_SERIAL)..."
	adb -s $(ADB_SERIAL) install -r "$(POSTIT_APK)" -r

qemu-uninstall:
	adb -s $(ADB_SERIAL) uninstall fr.pschneider.PostIt

# Dump recent logcat output for the running PostIt.Android process.
# By default, prints the last $(LOGCAT_LINES) lines (one-shot, with
# `-d`). Set LOGCAT_FOLLOW=1 to follow the stream live instead.
# Filtering is by PID (pidof fr.pschneider.PostIt), not by tag,
# because Mono/Xamarin can emit logs under several tags
# (mono, PostIt.Android, Avalonia.Android) and tag-based filtering
# would miss the ones not matching. PID-based filtering is exact.
# If the app is not running, pidof returns empty and logcat exits
# silently with no output; that is the expected behaviour for
# "no logs yet".
qemu-logcat:
	@PID=$$(adb -s $(ADB_SERIAL) shell pidof fr.pschneider.PostIt 2>/dev/null | tr -d '\r\n'); \
	if [ -z "$$PID" ]; then \
	    echo "  fr.pschneider.PostIt is not running on $(ADB_SERIAL)."; \
	    echo "  Start the app first (am start -n fr.pschneider.PostIt/PostIt.Android.PostItMainActivity)"; \
	    exit 1; \
	fi; \
	echo "  Following PID $$PID (LOGCAT_FOLLOW=$(LOGCAT_FOLLOW), LOGCAT_LINES=$(LOGCAT_LINES))"; \
	if [ "$(LOGCAT_FOLLOW)" = "1" ]; then \
	    adb -s $(ADB_SERIAL) logcat -v time --pid=$$PID fr.pschneider.PostIt:F; \
	else \
	    adb -s $(ADB_SERIAL) logcat -d -v time -t $(LOGCAT_LINES) --pid=$$PID fr.pschneider.PostIt:F; \
	fi

# Clear logcat, launch PostIt.Android, then dump everything that was
# emitted during the startup window. Targets the "démarrage KO" case
# where the process starts but Avalonia never renders a frame — the
# logcat trace from process start to first frame is what diagnoses it.
#
# Override LOGCAT_BOOT_WAIT to extend the post-launch wait
# (default 15s; raise to 30+ if the device is slow to boot Avalonia).
LOGCAT_BOOT_WAIT ?= 15
qemu-logcat-boot:
	@echo "  Clearing logcat buffer..."
	adb -s $(ADB_SERIAL) logcat -c
	@echo "  Launching fr.pschneider.PostIt..."
	adb -s $(ADB_SERIAL) shell am start \
	    -n fr.pschneider.PostIt/PostIt.Android.PostItMainActivity
	@echo "  Waiting $(LOGCAT_BOOT_WAIT)s for the app to start rendering..."
	@sleep $(LOGCAT_BOOT_WAIT)
	@echo "  Dumping logcat (PostIt PID + system buffer):"
	@PID=$$(adb -s $(ADB_SERIAL) shell pidof fr.pschneider.PostIt 2>/dev/null | tr -d '\r\n'); \
	if [ -n "$$PID" ]; then \
	    echo "  (PID $$PID at dump time)"; \
	    adb -s $(ADB_SERIAL) logcat -d -v time --pid=$$PID; \
	else \
	    echo "  (PostIt process not running at dump time — dumping last $(LOGCAT_LINES) lines unfiltered)"; \
	    adb -s $(ADB_SERIAL) logcat -d -v time -t $(LOGCAT_LINES); \
	fi

qemu: qemu-run qemu-wait-boot qemu-install
	@echo "  ✓ PostIt.Android installed on $(ADB_SERIAL)"

.PHONY: test release qemu qemu-run qemu-stop qemu-wait-boot qemu-build qemu-install qemu-logcat qemu-logcat-boot
