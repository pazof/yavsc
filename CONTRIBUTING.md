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
