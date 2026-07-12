# Stratégie de test

Yavsc utilise **xUnit** (`xunit.v3`) avec un mix d'unitaire pur
et d'intégration légère. Les projets de tests sont sous
`src/<projet>.Tests/` et consomment le scaffold partagé
`src/Yavsc.Tests.Shared/`.

## Vue d'ensemble

| Sujet | Document |
|---|---|
| Scaffold partagé (`WebHostFixture`, JWT de test, etc.) | [src/Yavsc.Tests.Shared/README.md](../src/Yavsc.Tests.Shared/README.md) |
| Convention des dossiers de tests | [Conventions](#conventions-des-dossiers-de-tests) |
| Driver EF Core en test | [EF Core en test](#ef-core-en-test) |
| Stubs d'authentification et de permissions | [Auth et permissions](#auth-et-permissions) |

## Conventions des dossiers de tests

Sous `src/<projet>.Tests/`, on trouve quatre dossiers de premier
niveau qui classifient les tests par intention :

| Dossier | Usage |
|---|---|
| `NonRegression/` | Régressions : un bug constaté, un test qui le détecte si on le réintroduit |
| `Mandatory/` | Tests bloquants : ils doivent passer avant tout merge |
| `Smoke/` | Smoke tests HTTP rapides, montent un host léger |
| `Controllers/` | Tests unitaires des contrôleurs (mock du service, assertions sur le mapping HTTP) |

Les `NonRegression` sont la cible par défaut quand on fixe un
bug : ils doivent être **rouges avant le fix, verts après**, et
continuer à **casser** si quelqu'un revert le fix. Pas de test
qui passe à vide.

## EF Core en test

Pour les tests qui ont besoin d'un `ApplicationDbContext`, on
utilise **`UseInMemoryDatabase`** avec un `InMemoryDatabaseRoot`
partagé au niveau de la fixture. Pas de SQLite, pas de Docker,
pas de mock du contexte : le service testé s'exécute contre
un vrai `DbContext` sur in-memory.

```csharp
private static readonly InMemoryDatabaseRoot _dbRoot = new();

var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase("Yavsc.Org.Tests.MyFixture", _dbRoot)
    .Options;
```

Le `InMemoryDatabaseRoot` partagé est important : sans lui, EF
crée un store indépendant par `DbContext` dans certaines
configurations, et un test qui seed + read sur deux contextes
voit un store vide. Le pattern est documenté dans
`BlogsWebServerFixture` ([src/Yavsc.Blogs.Tests/BlogsWebServerFixture.cs](../src/Yavsc.Blogs.Tests/BlogsWebServerFixture.cs)).

> **Limite connue** : le provider in-memory **ignore** les
> `Migration` EF et ne respecte pas les FK **sur les raw
> SQL** (`ExecuteSqlRaw`). Pour tester des contraintes FK, on
> écrit la configuration dans `OnModelCreating` et on s'appuie
> sur le fait qu'EF la respecte à l'`Add`/`SaveChanges`. Pour
> tester des migrations, c'est l'environnement de staging.

## Auth et permissions

L'authorization policy provider de prod est swappé contre
`TestAuthPolicyProvider` (dans `Yavsc.Tests.Shared`) par les
fixtures spécialisées. Les tests qui ont besoin qu'un user soit
"Administrator" envoient un header `X-Test-Rôle` ; ceux qui
veulent un user anonyme omettent le header.

Pour les tests unitaires qui n'ont pas besoin du pipeline
HTTP, on stub `IAuthorizationService` directement (cf.
`BlogspotController` dans `Yavsc.Org.Tests/NonRegression/`)
pour éviter de monter un host complet.

## Quand ne PAS écrire de test

Un test qui ne détecte rien n'est pas un test. Si l'invariant
qu'on cherche à protéger est déjà enforced par EF, par le
compilateur, ou par une couche applicative en amont, le test
est du bruit. Mieux vaut :
- Un test qui assert un **comportement observable** (code
  retour HTTP, exception typée, valeur de retour)
- Ou pas de test, et une note dans le code

La non-régression se prouve par un test qui casse si on
réintroduit le bug. Pas par un test qui passe aujourd'hui et
qui continuera à passer après un revert.
