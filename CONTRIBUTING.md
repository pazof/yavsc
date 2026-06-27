# Contribuer à Yavsc

## Pré-requis

- .NET 10 SDK
- PostgreSQL (≥ 14) — local ou via `docker compose up`
  (cf. `docker-compose.yaml`)
- Node.js (uniquement pour la toolchain Avalonia Browser/WASM)
- Pour Android : Android SDK + workload `dotnet workload install android`

## Premier build

```bash
git clone https://github.com/pazof/yavsc.git
cd yavsc
dotnet restore
dotnet build
```

Pour exécuter Yavsc.Org en local :

```bash
cp src/Yavsc.Org/appsettings-org.json src/Yavsc.Org/appsettings-org.Development.json
$EDITOR src/Yavsc.Org/appsettings-org.Development.json   # renseigner Site.Authority, ConnectionStrings.YavscConnection, …
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Yavsc.Org
```

Cf. [doc/Architecture.md](./doc/Architecture.md) section
"Paramétrage" pour le détail des variables à renseigner.

## Tests

```bash
dotnet test
```

Les tests sont répartis en :

- `src/Yavsc.Org.Tests/` — tests d'isolation du front web
- `src/PostIt.Tests/` — tests du client desktop PostIt

## Conventions de code

Le repo applique `.editorconfig` (UTF-8, LF, `indent_size = 4` en
C#, `2` en Razor / XML / `.axaml`). Pas de formatter dédié — les EDI
(Visual Studio, Rider) appliquent automatiquement les règles Roslyn
+ EditorConfig.

Quelques règles non capturées par `.editorconfig` :

- `using` triés par groupe (System, puis packages, puis local).
  Pas de séparation visuelle entre groupes (cf.
  `dotnet_separate_import_directive_groups = false`).
- Préférer les types BCL (`int`, `string`) aux types framework
  (`Int32`, `String`).
- Préférer les expressions de pattern matching aux casts explicites.

## Branches & commits

- Trunk-based sur `main`. Pas de branche longue durée pour
  le moment ; on lande directement sur `main` via PR (quand le
  projet attire de l'attention) ou push direct (mode solo).
- Un commit = un changement logique. Regrouper les fichiers qui
  touchent au même flux dans un seul commit (login + tests + doc
  = un commit), mais **séparer** les commits de reformulation
  d'historique (« refactor », « typo ») des commits de feature
  dans la mesure du possible.
- Messages de commit en anglais, format :

  ```
  <scope>: <imperative summary>

  <body — what changed and why, not how>
  ```

  Le scope est le nom du sous-projet (`postit`, `doc`, `readme`,
  `yavsc.org`, `yavsc.api`, …). Pour les commits qui touchent
  plusieurs sous-projets, préférer un scope générique (`readme:`,
  `doc:`).

## Découpage des projets

Cf. [doc/architecture/decoupage-organisation.md](./doc/architecture/decoupage-organisation.md).

qui couvre à la fois le build, la publication et les images
runtime, plus un second Dockerfile minimal (`Dockerfile.backend`)
utilisé uniquement par le workflow de publication de l'image
de production.

| Dockerfile              | Stages                                                          | Construit par                                |
|-------------------------|-----------------------------------------------------------------|----------------------------------------------|
| `Dockerfile`            | `build-env` (default), `publish-org`/`api`/`blogs`, `web-runtime`, `api-runtime`, `blogs-runtime` | `docker compose build` + `.github/workflows/docker-publish-android.yml` |
| `Dockerfile.backend`    | Idem limité à `build-env` + `publish-org` (suffisant pour publier l'image `pazof/yavsc`) | `.github/workflows/docker-publish-backend.yml` |

L'image de base est construite depuis le dépôt sibling
`dotnet-android-build-image` (Debian 12 + .NET 10 SDK + Android
SDK 36 + workload .NET Android). Elle est poussée sur Docker Hub
sous le tag `pazof/yavsc-build-env:debian12-dotnet10-android36-v1`.
Le tag est déclaré comme `ARG BUILD_ENV_TAG` au début du
`Dockerfile` (et de `Dockerfile.backend`) — il faut le bumper en
lockstep dans les deux fichiers **et** dans `docker-compose.yaml`
(chaque bloc `build.args.BUILD_ENV_TAG`) quand l'image de base
est reconstruite.

### `docker compose up`

```bash
sudo docker compose up --build
```

Démarre 4 services : `db` (PostgreSQL 16), `web` (Yavsc.Org),
`api` (Yavsc.Api), `blogs` (Yavsc.Blogs). Chaque service runtime
pointe sur le stage correspondant du Dockerfile multi-stage via
`build.target`. Les services runtime attendent le healthcheck
`pg_isready` de `db` avant de démarrer.

### appsettings-org.json

Le fichier de configuration de prod (`src/Yavsc.Org/appsettings-org.json`)
n'est **pas** commité. Il est injecté dans chaque image runtime
via un **BuildKit secret mount** — le fichier reste sur l'hôte,
n'apparaît dans aucun layer :

```yaml
secrets:
  yavsc_appsettings:
    file: ./src/Yavsc.Org/appsettings-org.json
```

Compose le passe automatiquement à `docker build` via le bloc
`build.secrets` de chaque service runtime.

### HTTPS en production

En dev local les services runtime sont forcés à **HTTP seul** par
un bloc `environment` explicite dans `docker-compose.yaml` :

```yaml
environment:
  ASPNETCORE_URLS: "http://+:5000"
  ASPNETCORE_HTTPS_PORT: ""
```

C'est nécessaire parce que `appsettings-org.Development.json`
positionne `Site.Authority = https://localhost:5001`, ce qui
pousse Kestrel à essayer de binder HTTPS même sans certificat
disponible — et échoue proprement avec « Unable to configure
HTTPS endpoint. No server certificate was specified ».

En production, sur chaque service runtime de `docker-compose.yaml` :

1. Remplacer l'`environment.ASPNETCORE_URLS` par la forme double-bind
   `http://+:5000;https://+:5001` (ou équivalent pour Api / Blogs).
2. Décommenter le port HTTPS correspondant (`5001` pour Org,
   `5003` pour Api, `5005` pour Blogs).
3. Décommenter le volume `/etc/letsencrypt:/etc/letsencrypt:ro` —
   monter le répertoire Let's Encrypt de l'hôte en lecture seule
   pour que Kestrel accède aux fichiers `.pem`.
4. Dans `appsettings-org.json`, ajouter un bloc `Kestrel:Endpoints`
   pointant vers les chemins du volume monté. Exemple :

   ```json
   "Kestrel": {
     "Endpoints": {
       "Https": {
         "Url": "https://+:5001",
         "Certificate": {
           "Path": "/etc/letsencrypt/live/yavsc.example/fullchain.pem",
           "KeyPath": "/etc/letsencrypt/live/yavsc.example/privkey.pem"
         }
       }
     }
   }
   ```
5. Régénérer l'image runtime (les `appsettings` sont baked dans
   l'image via BuildKit secret mount — cf. section appsettings-org.json).

### Bumper l'image de build

Quand on bumpe Debian, .NET SDK ou Android SDK, reconstruire
l'image de base :

```bash
cd ../dotnet-android-build-image
docker build -t pazof/yavsc-build-env:debian12-dotnet10-android36-v2 .
docker push pazof/yavsc-build-env:debian12-dotnet10-android36-v2
```

Puis bumper en lockstep dans :
- `Dockerfile` (ARG `BUILD_ENV_TAG` en tête de fichier)
- `Dockerfile.backend` (idem)
- `docker-compose.yaml` (chaque bloc `build.args.BUILD_ENV_TAG`).

### Vérifier un build isolé d'un stage runtime

```bash
docker build \
  --secret id=yavsc_appsettings,src=src/Yavsc.Org/appsettings-org.json \
  --target web-runtime \
  -t yavsc-org:dev .
docker run --rm -p 5000:5000 yavsc-org:dev
```

et une roadmap à jour dans [ROADMAP.md](./ROADMAP.md). Toute
modification de modèle doit être précédée d'une note DDD ; les BC
(Conciliation, etc.) listés dans ces docs sont les cibles de
conception.
## Sessions DDD

Le repo tient un journal de design DDD sous `doc/ddd-exploration-*.md`
et une roadmap à jour dans [ROADMAP.md](./ROADMAP.md). Toute
modification de modèle doit être précédée d'une note DDD ; les BC
(Conciliation, etc.) listés dans ces docs sont les cibles de
conception.

## Sécurité

Cf. la section "🔒 Hygiène transverse (à tous les jalons)" de
[ROADMAP.md](./ROADMAP.md). En particulier : ne jamais committer
de secret, utiliser `dotnet user-secrets` en développement et les
variables d'environnement `ASPNETCORE_*` en production.

## Questions

Ouvrir une issue GitHub, ou — pour les questions de design —
démarrer une session DDD et la consigner dans `doc/`.
