
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

.PHONY: test release
