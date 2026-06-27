# Découpage des projets .NET

> **Récapitulatif** : Le code est réparti en sept projets .NET
> sous `src/`, organisés par rôle : domaine partagé, bibliothèque
> métier, front web utilisateur-facing, API REST, blog, front admin,
> et tests. Détail dans cette page, racine de l'architecture :
> [Architecture.md](../Architecture.md).

## Vue d'ensemble

```
                     ┌──────────────────────────────┐
                     │ Yavsc.Abstract (lib)         │
                     │ Modèles du domaine partagés  │
                     └──────────────┬───────────────┘
                                    │
                     ┌──────────────▼───────────────┐
                     │ Yavsc.Server (lib)           │
                     │ Helpers, EF, services        │
                     └──────┬───────────────┬───────┘
                            │               │
              ┌─────────────▼──┐    ┌───────▼────────────┐
              │ Yavsc.Org (web)│    │ Yavsc.Api (web)    │
              │ Front web      │    │ API REST JSON      │
              │ + Identity-    │    │ JwtBearer          │
              │   Server       │    │                    │
              │ + Razor views  │    └────────────────────┘
              └────────────────┘
                            │
                     ┌──────▼────────────────────────────┐
                     │ Yavsc.Blogs (web)                 │
                     │ Front web spécifique au blog      │
                     │ (consomme Yavsc.Server)           │
                     └───────────────────────────────────┘

Clients externes :
  - Yavsc.Web  : front admin / expérimentation, JwtBearer.
  - PostIt     : client desktop Avalonia (cf. postit-oidc.md).
```

## Par projet

| Projet             | Type SDK       | Rôle                                                                                          |
|--------------------|----------------|-----------------------------------------------------------------------------------------------|
| `Yavsc.Abstract`   | Library        | Modèles du domaine partagés (entités, value objects, enums). Pas de dépendance framework.     |
| `Yavsc.Server`     | Library        | Bibliothèque métier : `DbContext`, helpers PayPal/SMTP, services métier, modèles billing/blog. |
| `Yavsc.Org`        | ASP.NET Web    | Front web utilisateur-facing : Razor views, IdentityServer8 (OP), AccountController, Razor.   |
| `Yavsc.Api`        | ASP.NET Web    | API REST JSON consommée par les clients externes (PostIt, …). JwtBearer auth.                |
| `Yavsc.Blogs`      | ASP.NET Web    | Sous-domaine front web spécifique au blog, autonome mais consomme `Yavsc.Server`.            |
| `Yavsc.Web`        | ASP.NET Web    | Front admin / expérimentations, JwtBearer, référence `Yavsc.Abstract` et `Yavsc.Server`.    |
| `Yavsc.Org.Tests`  | Test (xUnit)   | Tests d'isolation du front web (`Yavsc.Org`) — fakes, controller tests.                     |

## Pourquoi ce découpage

- **`Yavsc.Abstract` séparé de `Yavsc.Server`** : permet aux clients
  externes (PostIt — projet Avalonia sous `src/PostIt/`) de partager
  les modèles DTO sans embarquer Entity Framework ni ASP.NET Core.
- **`Yavsc.Server` est une library, pas un web host** : on peut
  tester les services sans démarrer Kestrel ; plusieurs fronts
  (Org, Api, Blogs) consomment les mêmes helpers sans dupliquer.
- **`Yavsc.Api` séparé de `Yavsc.Org`** : l'API REST est destinée
  aux clients headless (PostIt, intégrations tierces) ; elle n'a
  pas besoin des vues Razor ni d'IdentityServer. Inversement, le
  front web peut servir ses propres pages serveur sans exposer
  une API REST.
- **`Yavsc.Blogs` séparé** : le blog a son propre cycle de vie
  (déploiement, modèle de contenu) tout en réutilisant le contexte
  EF de `Yavsc.Server`.

## Conséquences pratiques

- Une modification d'un modèle DDD dans `Yavsc.Abstract` peut
  casser **plusieurs fronts à la fois** (Org, Api, Blogs, Web).
  C'est attendu ; les tests de `Yavsc.Org.Tests` doivent suivre.
- Ajouter une route API dans `Yavsc.Api` n'implique pas de
  recompiler `Yavsc.Org` (et inversement).
- `PostIt` (sous `src/PostIt/`) ne référence que `Yavsc.Abstract` ;
  il n'embarque ni EF ni ASP.NET Core.

## Voir aussi

- [Architecture.md](../Architecture.md) — racine.
- [postit-oidc.md](postit-oidc.md) — PostIt consomme `Yavsc.Api`
  via OIDC.
