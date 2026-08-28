# Changelog

Toutes les modifications notables de PostIt et de la plateforme Yavsc
sont documentées dans ce fichier.

Le format suit [Keep a Changelog](https://keepachangelog.com/fr/1.1.0/),
et ce projet adhère au [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

À noter : la **parité du numéro de patch** porte une signification de canal :

- **patch pair** (ex. `1.0.0`, `1.0.2`) → **stable**
- **patch impair** (ex. `1.0.1`, `1.0.3`) → **preview**
- **suffixe** (ex. `1.0.0-rc1`, `1.0.0-alpha`) → **instable**

Cette convention est partagée avec le dépôt
[`postit-debian`](https://forgejo.pschneider.fr/notazof/postit-debian)
pour la production des paquets `.deb`.

## [1.0.8-rc4] - unstable

### Added

### Changed

### Fixed

* [TODO] bug loading a blog post from PostIt, ACL come along with and don't need any "Refresh" button.

## [1.0.8-rc3] - unstable

### Added

* a code cleanup,
* a first Xamarin.UITest is successful, but disabled, because breaking the actual CI process,
* Android app starts, the login process succeeds

### Changed

L'identifiant de l'application client Android a changé, il passe en minuscules :
`fr.pschneider.postit`

### Fixed

nothing, [TODO] a bug persists laoding a blog post from PostIt, ACL sould come along with and should not need any "Refresh" button.
Today, assigning ACL succeeds the first time, the following times result in a 409

## [1.0.8-rc1] - unstable

### Added
- `BlogAclApiTests.PostCircleAuthorization_returns_201_when_payload_mirrors_PostIt_shape_against_existing_circle_named_test`
  : test de non-régression qui épingle la forme exacte du payload
  que PostIt envoie à `POST /api/v1/blogacl` (un objet
  `PostAccessControlRulePayload` avec `CircleId` et `BlogPostId`).
  C'est le verrou côté test du fix applicatif PostIt + serveur.
- `BlogAclApiTests.PostCircleAuthorization_never_returns_500` : une
  `[Theory]` couvrant quatre shapes de payload (`{ circleId }`,
  corps vide, `{ blogPostId }` seul, `{ circleId, blogPostId: 0 }`)
  qui doivent tous retourner un statut différent de 500. Toute
  réintroduction d'un chemin 500 dans le futur fera rougir ce test.
- `BlogAclApiTests.PostCircleAuthorization_dosent_return_500` et
  `..._dosent_return_500_on_success` : entry points `[Fact]` qui
  appellent la `[Theory]` ci-dessus avec un payload spécifique
  chacun, pour pouvoir filtrer en isolation depuis la ligne de
  commande ou le CI.
- Règle « Pas de `object` dans le code source applicatif » ajoutée
  à `CONTRIBUTING.md` : types de retour, paramètres, champs,
  propriétés, variables locales doivent être typés statiquement.
  `dynamic` est interdit pour les mêmes raisons.

### Changed
- `BlogAclApiController.CheckOwner` devient `CheckOwnerAsync` et
  utilise `FirstOrDefaultAsync` au lieu de `First`, supprimant
  l'appel LINQ synchrone sur le fil de la requête et retournant
  `false` sur cercle manquant (le contrôleur mappe déjà cela vers
  `ChallengeResult`).
- `BlogsWebServerFixture` seed `alice`, son `Circle` et son
  `BlogPost` une seule fois au démarrage du host, sur la
  `SqliteConnection` partagée (`Cache=Shared`). Le précédent
  `EnsureDeleted` au début de chaque test fermait la connexion
  statique et détruisait le store `:memory:` pour tous les autres
  `DbContext` ; il est retiré au profit d'un `EnsureCreated`
  idempotent.

### Fixed
- `POST /api/v1/blogacl` ne retourne plus 500 sur les payloads
  dont `BlogPostId` est absent ou à zéro. Le contrôleur rejette
  `BlogPostId <= 0` avec `400 BadRequest` avant que la requête
  n'atteigne `SaveChangesAsync`. L'incident de prod du 2026-08-21
  sur mercure (PostIt envoyant seulement `circleId`, le serveur
  voyant `BlogPostId = default(long) = 0` et EF Core levant
  `InvalidOperationException` sur l'INSERT) n'est plus atteignable.
- PostIt `PostAclDialogViewModel.AddAsync` envoie désormais le
  payload explicite `PostAccessControlRulePayload { CircleId,
  BlogPostId }` au lieu de l'ancien `CircleAuthorization {
  CircleId }`. Le DTO serveur `PostAccessControlRulePayload` est
  introduit dans `Yavsc.Abstract` pour porter le contrat.

## [1.0.7] - preview

### Added
- Per-post ACL in PostIt: a new “Manage ACL” page, opened from the ACL
  button on a selected post, lets the post author grant or revoke
  grants for individuals or circles. The server scopes each grant
  operation to `caller == post.AuthorId` and returns `404` (not `403`)
  for posts the caller does not own, so the existence of another
  user's post is not leaked.
- Circle membership API + UI: three new REST endpoints under
  `/api/circle/{id}/members` (`GET` list, `POST` add, `DELETE`
  remove) and a new “Members” column on the *My Circles* page with an
  “Add a member” button that opens a search modal. The search modal
  reuses `IUserDirectory` (introduced by the `IContactService` split
  in this same release) — exactly the use case the abstraction was
  carved out for.
- Publish toggle for blog posts: a new `PUT /api/BlogApi/{id}/publish`
  endpoint, and a `Published` checkbox in the post toolbar that
  toggles a `BlogSpotPublication` row for the post. The publish
  signal flows through the pre-existing `PermissionHandler.IsPublic`
  path, so no new column was needed and the server-side authorisation
  logic is unchanged.
- `UserSearchApiController` in `Yavsc.Blogs`:
  `GET /api/user-search?q=...&e=...&take=...`. Any-authenticated-
  caller endpoint that exposes the user's email under a closed-
  community assumption (documented in the controller's XML doc).
  Wired to the PostIt Desktop address book so the user search modal
  picks it up.
- `IYavscApiClient` abstraction in `Yavsc.Api.Client`. The transport
  for the blog/circle/blog-acl/user-search clients is now accessed
  through this interface, so `PostIt.Tests` can stub the HTTP layer
  without spinning up a real WebAPI host.
- Forgejo Actions release workflow: a `.forgejo/workflows/release.yml`
  pipeline that builds and publishes a release with the PostIt APK
  on tag push. Written in pure bash (the runner image has no Node),
  uses `jq` for JSON body construction and response parsing, uses the
  runner-provided `GITHUB_TOKEN` (no repo-level secret needed),
  validates the CHANGELOG section heading before allowing the tag
  to ship.
- `make release V=<version>` target: creates a `release/<V>` branch
  from `main`, bumps the `<Version>` property in every `.csproj` via
  `dotnet-gitversion /updateprojectfiles`, commits the bump on the
  release branch, and pushes to `origin`. Fails fast if the working
  tree is dirty or if `HEAD` is not on `main`.
- Forgejo status badges in the README.

### Changed
- The new Publish toggle replaces the “Visibility enum” approach
  originally drafted in this branch: the existing `BlogSpotPublication`
  table already carried enough information to expose a publish
  switch, so no schema change was needed. The original `feat(blog):
  add Visibility { Private, Public }` commit and its EF migration
  were reverted in favour of the endpoint-only toggle.
- `BlogPost` DTO and `IBlogPost` moved from `PostIt.Models` to
  `Yavsc.Abstract.Blogspot`, the shared assembly where the server-side
  entity and the wire DTO both live. Renamed `Yavsc.Blogspot.BlogPost`
  to `BlogPostDto` to make the wire/entity distinction explicit.
- `BlogAclApiController` and `CircleApiController` moved from
  `Yavsc.Api` (not yet enabled in production) to `Yavsc.Blogs`, where
  they belong next to the `BlogSpotService` they depend on.
- `IContactService` split from `IUserDirectory`: the two interfaces
  previously conflated the local address-book access (mobile-only,
  via `Contacts.Default`) and the Yavsc user-search access
  (Desktop-only, via `/api/user-search`) behind a single facade. The
  split restores the `ContactDto.Emails` multi-value shape that was
  being silently flattened to a single string before.
- CI: the Forgejo Actions build now compiles `.csproj` projects
  directly inside the runner container (which ships the .NET SDK +
  Android workload), instead of relying on a separate Docker build
  step. Node-based third-party actions were replaced with bash + curl
  + `jq`. The validate-release job parses the CHANGELOG section
  heading to derive the channel (`stable` / `preview` / `unstable`)
  rather than the patch-version parity alone.

### Fixed
- `CircleApiController` used to read the caller's user id via
  `FindFirstValue(ClaimTypes.NameIdentifier)`, which does not match
  when JWT Bearer middleware has `MapInboundClaims = false`. Switched
  to `User.GetUserId()` (tries `sub` first, then
  `ClaimTypes.NameIdentifier`, then `nameid`). This was a latent
  bug visible in tests but easy to ship to production if a host
  ever disabled the remap.
- `CircleApiController` and `BlogAclApiController` reads and writes
  were not always scoped to the caller's own data. Tightened the
  authorisation checks: cross-user reads now return `404`, not the
  raw record.
- `validate-release` CHANGELOG channel check used to parse the
  patch-version parity only, which disagreed with the channel
  suffix in the section heading (e.g. `## [1.0.7] - preview`
  would be flagged as `stable` from the parity alone). The job now
  inspects the heading line and trusts the suffix when present.
- `.forgejo/workflows/release.yml`: the asset-upload URL now carries
  the asset name as a query-string parameter instead of a `curl`
  positional argument. The previous shape triggered Forgejo's
  “Missing `name` parameter” 400 in some cases.

### Removed
- The `## [Unreleased]` block has been moved into this section.
- The abandoned `Visibility { Private, Public }` enum and its EF
  migration, reverted in this release. The publish toggle covers
  the same user-visible switch without a schema change.

[Unreleased]: https://github.com/pazof/yavsc/compare/HEAD
[1.0.8-rc1]: https://github.com/pazof/yavsc/compare/1.0.7...1.0.8-rc1
[1.0.7]: https://github.com/pazof/yavsc/compare/1.0.6...1.0.7
[1.0.6]: https://github.com/pazof/yavsc/compare/1.0.5...1.0.6

## [1.0.6] - stable

### Added
- Self-hosted Forgejo Actions runner now drives the CI build for the
  yavsc repository, using the
  `pazof/yavsc-build-env:debian12-dotnet10-android36-v2` image pulled
  from Docker Hub. Workflow runs end-to-end: clone, restore, build,
  test, with NuGet.config picking up the `isn.pschneider.fr` feed.
- The build-env image now ships `jq` (Debian package, ≥ 1.7), so the
  release workflow can build JSON bodies and parse API responses
  without a hand-rolled `sed`-based extractor that was matching the
  wrong `id` field on minified responses.

### Changed
- CI workflow `.forgejo/workflows/buildAndTest.yml` no longer relies on
  `actions/checkout` (the runner image has no Node); clones yavsc via
  `git`, fetches the ref under test, and initializes submodules over
  HTTPS.

### Fixed
- `Dockerfile` and `Dockerfile.backend` no longer carry a redundant
  `dotnet nuget add source` step that conflicted with the GitHub
  Actions APK build (`--allow-insecure-connections` on an HTTPS
  endpoint, exit 1). `NuGet.config` at the repo root supplies the
  `isn.pschneider.fr` feed for every restore, including inside Docker.
- `.forgejo/workflows/release.yml`: PATCH on `/releases/{id}` no longer
  404s on existing releases. The previous `sed`-based `json_field`
  matched the last `id` on the line (the author's), so it tried to
  PATCH `/releases/1` (the first user of the instance) instead of the
  actual release id. Switched to `jq` for both body construction and
  field extraction.

[1.0.6]: https://github.com/pazof/yavsc/compare/1.0.5...1.0.6
