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

## [Unreleased]

### Added

### Changed

### Fixed

### Removed

## [1.0.6] - stable

### Added
- Self-hosted Forgejo Actions runner now drives the CI build for the
  yavsc repository, using the
  `pazof/yavsc-build-env:debian12-dotnet10-android36-v1` image pulled
  from Docker Hub. Workflow runs end-to-end: clone, restore, build,
  test, with NuGet.config picking up the `isn.pschneider.fr` feed.

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

[Unreleased]: https://github.com/pazof/yavsc/compare/HEAD
[1.0.6]: https://github.com/pazof/yavsc/compare/1.0.5...1.0.6
