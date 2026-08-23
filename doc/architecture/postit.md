# PostIt — Topologie, navigation, DI

> **Récapitulatif** : PostIt est le client Avalonia du projet
> Yavsc. C'est un code-base unique (`src/PostIt/PostIt/PostIt.csproj`)
> **multi-cible** vers trois front-ends distincts
> (`PostIt.Desktop`, `Postit.Android`, `PostIt.Browser`). Cette
> fiche couvre la topologie des projets, le DI, le `ViewLocator`
> custo et la navigation — c'est-à-dire tout ce que la fiche
> [postit-oidc.md](postit-oidc.md) ne détaille pas déjà (l'OIDC,
> le flow d'auth, la persistance des tokens). Détail dans cette
> page, racine de l'architecture : [Architecture.md](../Architecture.md).

## Surface : un code-base, trois front-ends

```
                       ┌────────────────────────┐
                       │ PostIt (lib)           │
                       │ src/PostIt/PostIt/     │
                       │ Pages, ViewModels,     │
                       │ Services, ViewLocator  │
                       │ (aucun rendu natif)    │
                       └──────┬───┬─────┬───────┘
                              │   │     │
              ┌───────────────┘   │     └────────────────┐
              │                   │                      │
   ┌──────────▼────────┐ ┌────────▼─────────┐ ┌──────────▼────────┐
   │ PostIt.Desktop    │ │ PostIt.Android   │ │ PostIt.Browser    │
   │ Avalonia.Desktop  │ │ Avalonia.Android │ │ Avalonia.Browser  │
   │ Linux/Windows     │ │ APK              │ │ WASM              │
   │ + custom scheme   │ │ + Chrome Custom  │ │ (no native proc)  │
   │   postit://       │ │   Tabs           │ │                   │
   │ + IBrowser custo  │ │ + IBrowser custo │ │                   │
   └───────────────────┘ └──────────────────┘ └───────────────────┘
```

Le code partagé vit dans `PostIt/`. Chaque front-end est un
**projet Satellite SDK** Avalonia qui ne contient que le
`Program.Main`, le `Platform.CreateBrowser`, et les manifestes
spécifiques (IntentFilter Android, `app.manifest` Desktop).
Toute la logique (VM, services, navigation, settings, OIDC) est
dans le code-base partagé.

## ViewLocator custo

Le `ViewLocator` (cf. `src/PostIt/PostIt/ViewLocator.cs`) est un
`IDataTemplate` Avalonia **explicitement câblé sur le
`IServiceProvider`** :

```csharp
public Control Build(object? data) => data switch
{
    MainPageViewModel   => _services.GetRequiredService<MainPage>(),
    Settings            => _services.GetRequiredService<SettingsPage>(),
    HomePageViewModel   => _services.GetRequiredService<HomePage>(),
    SignaturePageViewModel => _services.GetRequiredService<SignaturePage>(),
    null                => new TextBlock { Text = "No view for <null>" },
    _                   => new TextBlock { Text = $"No view for {data.GetType().Name}" }
};
public bool Match(object? data) => data is ViewModelBase;
```

**Pourquoi un custo, et pas le `ViewLocatorBase` par défaut
d'Avalonia.Mvvm ?** Pour deux raisons :

1. **Sortie du `Activator.CreateInstance`** — les pages
   PostIt sont enregistrées dans le DI et peuvent avoir des
   dépendances (par construction, aujourd'hui aucune, mais
   l'extension future est ouverte). Le `ViewLocatorBase`
   historique fait `new View()`, ce qui rend impossible
   l'injection et complique les tests.
2. **Filtrage par `ViewModelBase`** — `Match` n'accepte que les
   types dérivés de `ViewModelBase`. Toute tentative d'afficher
   un objet métier (par ex. un DTO de l'API Yavsc) tombe sur le
   `TextBlock` "No view for X", pas sur un crash Avalonia.

Le `ViewLocator` est ajouté aux `DataTemplates` de l'app dans
`App.OnFrameworkInitializationCompleted` :

```csharp
DataTemplates.Clear();
DataTemplates.Add(new ViewLocator(provider));
```

**Conséquence pratique** : pour qu'une nouvelle page soit
affichée par un `ContentControl` qui binde un ViewModel, il
faut *deux* enregistrements : la page en `AddTransient` (ou
`AddSingleton`) dans le DI, **et** une case dans le `switch`
de `ViewLocator.Build`. Si l'un manque, l'app affiche
"No view for X" sans crash.

## Composition root (`App.axaml.cs`)

`App.OnFrameworkInitializationCompleted` est le seul endroit où
le DI est construit. Ordre, dans cet ordre :

1. `new Settings()` + `settings.Load()` — lit
   `~/.config/PostIt/postit-settings.json` (ou le fallback
   embarqué dans `PostIt.dll`).
2. `new TokenStore(...)` + `new YavscApiClient(settings, tokenStore)`.
3. `new ServiceCollection()` + enregistrements en bloc.
4. `services.BuildServiceProvider()`.
5. `Settings.BindToServiceProvider(provider)` — pose le
   singleton statique pour les helpers hors-DI
   (`Settings.GetCurrent()`, `Settings.RequireCurrent()`).
6. `DataTemplates.Add(new ViewLocator(provider))`.
7. Branche `IClassicDesktopStyleApplicationLifetime` /
   `ISingleViewApplicationLifetime` (Browser/Android).

### Enregistrements DI

| Service                       | Lifetime   | Pourquoi                                                                                  |
|-------------------------------|------------|-------------------------------------------------------------------------------------------|
| `Settings`                    | **Singleton** | État partagé (`Loaded`, `IsDirty`, `Authentication`) — doit être unique.                  |
| `YavscApiClient`              | Singleton  | Porte le `TokenStore` et le cache de tokens ; un seul par process.                         |
| `BlogApiClient`               | Singleton  | Mapper stateless, partagé.                                                                  |
| `SettingsPage`                | **Singleton** | Une seule instance pour la vie de l'app : le `DataContext` est câblé une fois au boot, le push est idempotent (cf. section *Garde anti-empilement* ci-dessous). |
| `MainPage` / `HomePage` / `SignaturePage` | Transient | Résolution à la demande par le `ViewLocator`.                                              |
| `MainPageViewModel` / `HomePageViewModel` / `SignaturePageViewModel` | Transient | VM reconstruites à chaque navigation ; pas d'état partagé à conserver.                    |
| `SessionStatusViewModel` + `SessionStatusBanner` | Singleton + Transient | Le VM est un singleton (survit à la navigation), le bandeau est transient (réinstancié quand la fenêtre le recrée). |

> **Invariant** : `Settings` est **uniquement** un singleton. Un
> `AddTransient<Settings>()` supplémentaire (qui réécrase le
> singleton dans le container) ferait que chaque push de
> `SettingsPage` crée une instance vide, casse les bindings
> Authority/ClientId, et perd toute édition. Si tu dois toucher
> à cette table, *ne pas* ajouter de registration pour
> `Settings` ailleurs que la ligne `AddSingleton(settings)`.

## Navigation

Le host de navigation est un `NavigationPage x:Name="NavRoot"`
posé sur `MainWindow.axaml`. La pile est gérée par deux
mécanismes distincts :

1. **Nav utilisateur (VM-first)** : un ViewModel (souvent dans
   une commande `[RelayCommand]`) appelle
   `await ((App)App.Current!).PushPageAsync(targetVm).ConfigureAwait(true);`.
   `App.PushPageAsync` (`src/PostIt/PostIt/App.axaml.cs`)
   résout la `Control` correspondante via le `ViewLocator`
   enregistré dans `Application.DataTemplates`, l'identifie
   comme `Page`, lui assigne le VM comme `DataContext`, et
   appelle `NavRoot.PushAsync(page)`. C'est le seul chemin
   pour les boutons de la toolbar, les `OpenSettings` /
   `OpenCircles` / `ManageAcl` / `OpenSignatureDev`, et
   toute autre nav déclenchée par un ViewModel.

2. **Signaux de cycle de vie** : le `SessionStatusViewModel`
   lève des événements consommés dans
   `App.OnFrameworkInitializationCompleted` pour orchestrer
   la nav de boot :

   | Événement           | Effet                                                            |
   |---------------------|------------------------------------------------------------------|
   | `LoginSucceeded`    | `PushAsync(MainPage)` au-dessus de `HomePage` (post-login).      |
   | `LogoutCompleted`   | `PopToRootAsync()` (revient à `HomePage`).                        |

   Ces events ne sont **pas** un canal de nav utilisateur ; ils
   portent une transition d'état applicatif (authentification
   établie / perdue) et c'est `App` qui choisit d'en faire une
   transition de pile.

### Garde anti-empilement

`NavigationPage.PushAsync` n'est pas idempotent : pousser deux
fois la même instance l'empile deux fois, et l'utilisateur doit
taper **Retour** N fois pour sortir. La garde est implémentée
dans `App.PushPageAsync` (et consommée par tous les chemins
de nav utilisateur) :

```csharp
var stack = window.NavRoot.NavigationStack;
if (stack.Count > 0 && ReferenceEquals(stack[stack.Count - 1], page))
{
    return Task.CompletedTask;  // déjà au sommet, no-op silencieux
}
return window.NavRoot.PushAsync(page);
```

La comparaison est par référence, pas par type : on ne veut
empêcher qu'un push de *cette* instance particulière, pas
celui d'une éventuelle autre `SettingsPage` (il n'en existe
qu'une, mais l'invariant est plus clair comme ça). La garde
repose sur le fait que `SettingsPage` est un singleton ; si on
repassait en `Transient`, `ReferenceEquals` resterait correct
mais la pertinence de la garde s'évaporerait (chaque push
apporterait une nouvelle instance et l'anti-empilement
reposerait sur l'invariant « la même est déjà au sommet »,
qui ne tiendrait plus).

## ViewModels et invariants d'état

- `Settings` est un objet-modèle exposé comme `DataContext`
  des pages. Il n'hérite pas de `ViewModelBase` (c'est un
  POCO `[ObservableProperty]`-généré par
  `CommunityToolkit.Mvvm`). Le fait qu'il soit utilisé comme
  DataContext est un raccourci de composition acceptable ici,
  pas un pattern à généraliser.

- `SessionStatusViewModel` est le seul VM avec une durée de vie
  **process-entière** (singleton). Il survit à toutes les
  navigations, expose `HasValidSession` en continu, et porte
  les événements de cycle de vie consommés par `App` pour
  orchestrer la nav de boot (`LoginSucceeded`,
  `LogoutCompleted`). La nav utilisateur déclenchée par
  l'utilisateur passe par `App.PushPageAsync(vm)`, pas par
  un événement du `SessionStatusViewModel`.

- `MainPageViewModel` / `HomePageViewModel` /
  `SignaturePageViewModel` sont `Transient` — une nouvelle
  instance est créée à chaque push, l'ancienne est libérée
  quand la page est dépilée. Pas d'état partagé entre
  occurrences ; pour passer une donnée d'une page à l'autre,
  on passe par un singleton (souvent `YavscApiClient` ou
  `Settings`).

## Bindings XAML : conventions de nommage

Pour les `[RelayCommand]` (cf. `CommunityToolkit.Mvvm`), le
binding XAML reprend **le nom exact de la méthode, sans
suffixe** :

| Méthode C#            | Binding XAML                |
|-----------------------|-----------------------------|
| `Save()`              | `{Binding Save}`            |
| `SaveAsync()`         | `{Binding SaveAsync}`       |
| `LoginCommand()`      | `{Binding LoginCommand}` (nom littéral, *pas* de suffixe ajouté) |
| `Clear()`             | `{Binding Clear}`           |
| `CaptureAsync()`      | `{Binding CaptureAsync}`    |

**JAMAIS** `SaveCommand`, `SaveCmd`, `DoSave`, etc. Le source
generator `[RelayCommand]` émet une propriété `ICommand` du
même nom que la méthode. Un binding qui pointe vers une
propriété inexistante casse l'app au moment du câblage (le
bouton ne se câble pas, et selon la version ça peut faire
planter l'init de la page).

Référence canonique : `AGENTS.md`, section
"Avalonia + CommunityToolkit.Mvvm : conventions de binding
pour `[RelayCommand]`".

## Pages et leurs rôles

| Page                       | DataContext              | Rôle                                                                  |
|----------------------------|--------------------------|-----------------------------------------------------------------------|
| `MainWindow`               | `HomePageViewModel` (initial) | Host de la `NavigationPage`.                                          |
| `SessionStatusBanner`      | `SessionStatusViewModel` | Bandeau persistant en haut de la fenêtre, visible sur toutes les pages. Boutons Login / Logout / Paramètres. |
| `HomePage`                 | `HomePageViewModel`      | Page d'accueil publique.                                              |
| `MainPage`                 | `MainPageViewModel`      | Éditeur de post de blog (après login).                                |
| `SignaturePage`            | `SignaturePageViewModel` | Capture de signature (estimateur).                                    |
| `SettingsPage`             | `Settings`               | Édition de Authority / ClientId / Scopes / URLs API / Dark mode. Sauver via `Save` (RelayCommand). |

## Conséquences pratiques

- **Ajouter une page** : créer la View + le ViewModel +
  enregistrer les deux dans le DI **et** dans le `switch` de
  `ViewLocator.Build`. Oublier le `ViewLocator` est silencieux
  (juste un TextBlock "No view for X"), pas une exception.
- **Ajouter un événement global de navigation** (par ex.
  "Push après payment success") : ne pas capturer `MainWindow`
  ni `NavigationPage` depuis le VM. La nav passe par
  `App.PushPageAsync(vm)` dans tous les cas : soit le VM
  appelle la méthode directement depuis une commande
  (`[RelayCommand]`), soit un handler abonné à un événement
  d'un singleton (cf. `SessionStatusViewModel`) l'appelle.
  Garder les VMs découplés du
  `IClassicDesktopStyleApplicationLifetime`.
- **Modifier l'OIDC** : la fiche à lire est
  [postit-oidc.md](postit-oidc.md), pas celle-ci. Cette fiche
  ne ré-explique ni le flow, ni le pipe, ni le custom scheme.
- **Modifier les `Settings`** : ne pas casser le singleton
  (cf. invariant ci-dessus). Toute propriété présentationnelle
  ajoutée (par ex. `ScopeListText`) doit porter `[JsonIgnore]`
  pour ne pas polluer le format sur disque.

## Voir aussi

- [Architecture.md](../Architecture.md) — racine.
- [postit-oidc.md](postit-oidc.md) — flow OIDC, custom scheme,
  silent refresh, persistance des tokens.
- [decoupage-organisation.md](decoupage-organisation.md) —
  place de `PostIt` dans le découpage global des projets
  .NET du repo.
