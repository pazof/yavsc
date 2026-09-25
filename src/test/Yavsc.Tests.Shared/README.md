# Yavsc.Tests.Shared

Scaffold partagé pour les tests d'intégration ASP.NET Core de
Yavsc. Ce projet **n'est pas lui-même un projet de tests** — il
n'a pas xUnit ni de test runner. Il expose des fixtures
réutilisables que les projets de tests consommateurs
(`Yavsc.Org.Tests`, `Yavsc.Blogs.Tests`, etc.) héritent ou
instancient.

## Contenu

| Fichier | Rôle |
|---|---|
| `WebHostFixture.cs` | Base abstraite : Kestrel HTTPS, certificat auto-signé, port dynamique, host partagé inter-fixtures |
| `TestAuthPolicyProvider.cs` | `IAuthorizationPolicyProvider` de test, lit `X-Test-Role` au lieu d'interroger la DB |
| `TestTokenIssuer.cs` | Émet des JWT HS256 signés avec une clé statique, pour les tests d'API qui montent un `AddJwtBearer` réel |

## WebHostFixture

`WebHostFixture` est la base de toute fixture d'intégration.
Une seule instance de `WebApplication` tourne par process ; les
fixtures qui héritent partagent le host. Kestrel est bindé sur
`127.0.0.1:0` (port dynamique) avec un certificat auto-signé
généré lazily.

### Cycle de vie

- **Premier ctor** d'une fixture concrète → `InitializeAsync()`
  lance `BuildApp(builder)` puis `ConfigurePipelineAsync(app)`
  puis `app.StartAsync()`. L'`IServerAddressesFeature` est lu
  pour peupler `Addresses`.
- **Ctors suivants** (xUnit instancie une fixture par
  `IClassFixture<T>`) → reprise de l'état partagé via
  `CopySpecialisedSharedState()` (vide par défaut, surchargeable).
- **Dernier `Dispose`** → `app.StopAsync()`, reset des slots
  statiques.

### Hooks à surcharger

| Hook | Quand | Quoi y mettre |
|---|---|---|
| `BuildApp(builder)` | Toujours | Enregistrement des services, configuration in-memory, seeding éventuel |
| `ConfigurePipelineAsync(app)` | Optionnel | Pipeline middleware spécifique (sinon : pas de pipeline custom) |
| `CopySpecialisedSharedState()` | Optionnel | Recopie des slots statiques de la spécialisation sur les propriétés d'instance |

### Exemple : fixture de portée minimale

```csharp
public sealed class MyFixture : WebHostFixture
{
    protected override WebApplication BuildApp(WebApplicationBuilder builder)
    {
        // In-memory config, services, etc.
        return builder.Build();
    }
}
```

`MyFixture` n'a pas de test runner propre ; c'est l'assembly
consommateur (par exemple `Yavsc.MyModule.Tests`) qui déclare
les `[Fact]` et utilise `IClassFixture<MyFixture>`.

## Spécifications : fixtures concrètes

Deux fixtures héritent de `WebHostFixture` dans le repo :

### Yavsc.Org.Tests.WebServerFixture

Pour le host principal de Yavsc.Org. Caractéristiques :

- Configure `InMemory` pour la `ConnectionStrings` Yavsc
- Remplace `IAuthorizationPolicyProvider` par
  `TestAuthPolicyProvider` **avant** `ConfigureWebAppServices`
  (qui freeze la collection de services)
- Stub `ISmtpClientFactory` par `RecordingSmtpClientFactory`
  pour capturer les envois sans SMTP réel
- Seed IdentityServer8 : un `Client` + une `ApiScope` "test" +
  un `ApplicationUser` "Tester"
- Configure le pipeline via `app.ConfigurePipeline(...)` avec
  un manifeste de static assets explicite (le MSBuild target
  `CopyYavscOrgStaticAssets` du csproj miroir les manifests
  Yavsc.Org sous le nom Yavsc.Org.Tests.* dans le bin de test)

Cf. `src/Yavsc.Org.Tests/WebServerFixture.cs`.

### Yavsc.Blogs.Tests.BlogsWebServerFixture

Pour le host API de Yavsc.Blogs. Caractéristiques :

- `UseInMemoryDatabase("Yavsc.Blogs.Tests", _inMemoryRoot)` —
  un `InMemoryDatabaseRoot` partagé pour que POST + GET voient
  le même store
- `BlogSpotService` réel (pas de mock)
- `PermissionHandler` réel (le handler d'authorization qui
  résout `IsOwner(user, blog)`)
- `AddJwtBearer` réel avec HS256, validation contre
  `TestTokenIssuer.SigningKey` — pas d'OIDC discovery, pas
  d'IdP
- Politique `BlogScope` verbatim (`RequireAuthenticatedUser` +
  `RequireClaim("scope", "blogs")`)

Cf. `src/Yavsc.Blogs.Tests/BlogsWebServerFixture.cs`.

## TestAuthPolicyProvider

`IAuthorizationPolicyProvider` de test qui lit le rôle dans
l'en-tête HTTP `X-Test-Role` au lieu d'interroger la
`UserManager`. Permet aux smoke tests d'exercer `[Authorize
("AdministratorOnly")]` sans seed de rôle réel.

Activation : enregistré par les fixtures spécialisées **avant**
`ConfigureWebAppServices` (qui call `builder.Build()` et
fige la collection). La sémantique last-write-wins du
`AddSingleton` fait que le test provider prend le pas.

## TestTokenIssuer

Émet un JWT HS256 avec une `SigningKey` statique, exposé en
`TestTokenIssuer.SigningKey` (et `Issuer`). Les fixtures qui
montent un `AddJwtBearer` réutilisent cette clé pour valider
les tokens localement, sans OIDC discovery.

Helpers :

- `TestTokenIssuer.Issue(subject, scope, lifetime)` →
  chaîne `"Bearer <jwt>"` prête pour un header HTTP
- `TestTokenIssuer.SigningKey` — `SymmetricSecurityKey` à
  passer au `TokenValidationParameters` du `AddJwtBearer`

## Tests statiques sur du code compilé

Pour tester un display template Razor sans monter un host
ASP.NET, on peut s'appuyer sur la lecture du fichier source
et asserter des invariants syntaxiques. Cf.
`Yavsc.Org.Tests/NonRegression/ApplicationUserDisplayTemplateTests`
pour un exemple : on asserte que le cshtml ne porte plus
`Model.UserName` directement, ce qui aurait rouvert la
non-régression du 500 sur `/BlogSpot/Details/{id}`.

C'est pragmatique : la mise en place d'un `RazorProjectEngine`
pour compiler et rendre une vue hors host coûte plus cher que
ce qu'elle protège pour un seul template.

## Pour aller plus loin

- `doc/testing.md` à la racine : vue d'ensemble de la
  stratégie de test
- `src/Yavsc.Org.Tests/NonRegression/` et
  `src/Yavsc.Blogs.Tests/` : exemples d'utilisation
