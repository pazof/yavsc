# Découpage des projets .NET

> **Récapitulatif** : Le code est réparti en sept projets .NET
> sous `src/`, organisés par rôle : domaine partagé, bibliothèque
> métier, front web utilisateur-facing, API REST principale,
> backend API dédié aux blogs, front admin, et tests. Détail dans
> cette page, racine de l'architecture : [Architecture.md](../Architecture.md).

## Vue d'ensemble

```
                     ┌──────────────────────────────┐
                     │ Yavsc.Abstract (lib)         │
                     │ Modèles du domaine partagés  │
                     └──────────────┬───────────────┘
                                    │
                     ┌──────────────▼────────────────────┐
                     │ Yavsc.Server (lib)                │
                     │ Helpers, EF, services             │
                     └──────┬─────────┬──────────────┬───┘
                            │         │              │
              ┌─────────────▼──┐  ┌───▼──────────┐  ┌▼────────────────────┐
              │ Yavsc.Org (web)│  │ Yavsc.Api    │  │ Yavsc.Blogs (web)   │
              │ Front web      │  │ (web)        │  │ Backend API blogs   │
              │ + Identity-    │  │ API REST     │  │ (aucune vue Razor)  │
              │   Server       │  │ principale   │  │ déployé sur sous-   │
              │ + Razor views  │  │ JwtBearer    │  │ domaine en prod     │
              │ (héberge aussi │  └──────────────┘  └─────────────────────┘
              │  le front des  │
              │  blogs)        │
              └────────────────┘

Clients externes :
  - PostIt (Avalonia, code-base unique multi-cible) :
      · PostIt              — lib partagée (pages, VM, services)
      · PostIt.Desktop      — front-end Linux/Windows
      · PostIt.Android      — front-end APK
      · PostIt.Browser      — front-end WASM
    Cf. postit.md et postit-oidc.md.

Outils et tests :
  - cli                   — outillage CLI
  - Yavsc.Tests.Shared    — helpers de tests partagés
  - Yavsc.Org.Tests       — tests du front web
  - Yavsc.Blogs.Tests     — tests du backend blogs
  - PostIt.Tests          — tests du client PostIt
```

## Par projet

| Projet             | Type SDK       | Rôle                                                                                          |
|--------------------|----------------|-----------------------------------------------------------------------------------------------|
| `Yavsc.Abstract`   | Library        | Modèles du domaine partagés (entités, value objects, enums). Pas de dépendance framework.     |
| `Yavsc.Server`     | Library        | Bibliothèque métier : `DbContext`, helpers PayPal/SMTP, services métier, modèles billing/blog. |
| `Yavsc.Org`        | ASP.NET Web    | Front web utilisateur-facing : Razor views, IdentityServer8 (OP), AccountController. **Héberge aussi les vues (front) des blogs**. |
| `Yavsc.Api`        | ASP.NET Web    | API REST JSON principale consommée par les clients externes (PostIt, …). JwtBearer auth.     |
| `Yavsc.Blogs`      | ASP.NET Web    | **Backend API headless** dédié aux blogs (uniquement `*ApiController` + services + modèles — aucune vue Razor). Destiné à être déployé sur un sous-domaine en production, séparé du front web hébergé par `Yavsc.Org`. |
| `Yavsc.Org.Tests`  | Test (xUnit)   | Tests d'isolation du front web (`Yavsc.Org`) — fakes, controller tests.                     |
| `Yavsc.Blogs.Tests`| Test (xUnit)   | Tests d'isolation du backend blogs (`Yavsc.Blogs`).                                          |
| `Yavsc.Tests.Shared` | Library     | Helpers de tests partagés (fixtures, fakes, builders) entre les projets de tests.           |
| `PostIt`           | Library        | Code-base partagée du client PostIt (Avalonia) : pages, ViewModels, services, `ViewLocator` custo. Multi-cible — produit PostIt.Desktop / PostIt.Android / PostIt.Browser. |
| `PostIt.Desktop`   | Avalonia.Desktop | Front-end Desktop Linux/Windows : `Program.Main`, `Platform.CreateBrowser` (CustomSchemeBrowser), custom URI scheme `postit://`. |
| `PostIt.Android`   | Avalonia.Android | Front-end Android : `MainActivity` SingleTask, Chrome Custom Tabs, scheme `android://postit-signin`. |
| `PostIt.Browser`   | Avalonia.Browser | Front-end WASM : pas de process distinct, IBrowser N/A.                                     |
| `PostIt.Tests`     | Test (xUnit)   | Tests du client PostIt : settings, scopes Bearer, OIDC stub (`OidcStubAuthority`).           |
| `cli`              | exe / tool     | Outillage CLI (build, packaging, génération de clés).                                         |

## Pourquoi ce découpage

- **`Yavsc.Abstract` séparé de `Yavsc.Server`** : permet aux clients
  externes (PostIt — projet Avalonia sous `src/PostIt/`) de partager
  les modèles DTO sans embarquer Entity Framework ni ASP.NET Core.
- **`Yavsc.Server` est une library, pas un web host** : on peut
  tester les services sans démarrer Kestrel ; plusieurs fronts et
  backends consomment les mêmes helpers sans dupliquer.
- **`Yavsc.Api` séparé de `Yavsc.Org`** : l'API REST principale
  est destinée aux clients headless (PostIt, intégrations tierces) ;
  elle n'a pas besoin des vues Razor ni d'IdentityServer.
  Inversement, le front web peut servir ses propres pages serveur
  sans exposer une API REST.
- **`Yavsc.Blogs` séparé en backend headless** : le front des blogs
  (vues Razor) reste dans `Yavsc.Org` pour partager le rendu et
  l'auth avec le reste de l'application. Le backend, lui, est
  isolé dans son propre projet `Yavsc.Blogs` (uniquement des
  `*ApiController` + services + modèles, sans aucune vue Razor),
  destiné à être déployé sur un sous-domaine dédié en production
  — ex: `blogs.yavsc.example` — pour pouvoir scaler / restreindre
  l'API blog indépendamment du reste de la plateforme.

## Conséquences pratiques

- Une modification d'un modèle DDD dans `Yavsc.Abstract` peut
  casser **plusieurs fronts et backends à la fois** (Org, Api,
  Blogs, Web). C'est attendu ; les tests de `Yavsc.Org.Tests`
  doivent suivre.
- Ajouter une route API dans `Yavsc.Api` n'implique pas de
  recompiler `Yavsc.Org` (et inversement).
- `Yavsc.Blogs` et `Yavsc.Org` partagent le contexte EF via
  `Yavsc.Server` mais n'ont pas de dépendance l'un envers l'autre :
  la même base de données, deux hôtes distincts en production.
- `PostIt` (sous `src/PostIt/`) ne référence que `Yavsc.Abstract` ;
  il n'embarque ni EF ni ASP.NET Core.

## Voir aussi

- [Architecture.md](../Architecture.md) — racine.
- [postit-oidc.md](postit-oidc.md) — PostIt consomme `Yavsc.Api`
  via OIDC.
