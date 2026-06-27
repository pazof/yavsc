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

## Conteneurisation

Le repo expose trois images Docker :

| Dockerfile              | Cible                                                       | Construite par                                  |
|-------------------------|-------------------------------------------------------------|-------------------------------------------------|
| `Dockerfile`            | Image de build (Debian + .NET 10 + Android SDK 36). Sert aussi à produire l'APK Android. | `.github/workflows/docker-publish-android.yml` |
| `Dockerfile.backend`    | Idem, mais ne publie que `Yavsc.Org` (build + publish artifact). | `.github/workflows/docker-publish-backend.yml` |
| `Dockerfile.runtime*`   | Images runtime ASP.NET pour `Yavsc.Org` (5000), `Yavsc.Blogs` (5004), `Yavsc.Api` (5002). | `docker compose build`                          |

L'image de build est construite depuis le dépôt sibling
`dotnet-android-build-image` (Debian 12 + .NET 10 SDK + Android
SDK 36 + workload .NET Android). Elle est poussée sur Docker Hub
sous le tag `pazof/yavsc-build-env:debian12-dotnet10-android36-v1`.
Tous les `Dockerfile.runtime*` et les `Dockerfile` /
`Dockerfile.backend` référencent ce tag — le bumper en lockstep
quand l'image de build est reconstruite.

### `docker compose up`

```bash
sudo docker compose up --build
```

Cela démarre 4 services : `db` (PostgreSQL 16), `web` (Yavsc.Org),
`api` (Yavsc.Api), `blogs` (Yavsc.Blogs). Les services runtime
s'attendent via `depends_on.condition: service_healthy` sur le
healthcheck `pg_isready` de `db`.

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

En dev local on n'expose que HTTP. En production, sur chaque
service runtime de `docker-compose.yaml`, décommenter :

1. Le port HTTPS correspondant (`5001` pour Org, `5003` pour Api,
   `5005` pour Blogs).
2. Le volume `/etc/letsencrypt:/etc/letsencrypt:ro`.
3. Dans `appsettings-org.json`, renseigner
   `Kestrel:Certificates:Default:Path` et `:KeyPath` pour pointer
   vers les fichiers Let's Encrypt du volume monté.
4. Surcharger `ASPNETCORE_URLS` pour écouter à la fois HTTP et
   HTTPS.

### Bumper l'image de build

Quand on bumpe Debian, .NET SDK ou Android SDK, reconstruire
l'image de base :

```bash
cd ../dotnet-android-build-image
docker build -t pazof/yavsc-build-env:debian12-dotnet10-android36-v2 .
docker push pazof/yavsc-build-env:debian12-dotnet10-android36-v2
```

Puis bumper en lockstep dans :
- `Dockerfile`, `Dockerfile.backend`
- `Dockerfile.runtime`, `Dockerfile.runtime.blogs`, `Dockerfile.runtime.api`
- `docker-compose.yaml` (chaque bloc `build` qui pointe sur un
  `Dockerfile.runtime*`).

### Vérifier un build isolé d'une image runtime

```bash
docker build \
  --secret id=yavsc_appsettings,src=src/Yavsc.Org/appsettings-org.json \
  -f Dockerfile.runtime \
  -t yavsc-org:dev .
docker run --rm -p 5000:5000 yavsc-org:dev
```

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
