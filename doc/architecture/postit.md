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

## Reprise Android

`IActivityApplicationLifetime.MainViewFactory` crée une nouvelle `MainView`
pour chaque activité Android. Avant de remplacer la racine, PostIt relève
les types et les modèles de vue des pages de la pile précédente, puis
recrée ces pages dans le même ordre, sans animation. Les contrôles de
l'ancienne activité ne sont pas réutilisés ; les modèles de vue conservent
les sélections et brouillons en mémoire. Une nouvelle racine sans historique
reçoit la page d'accueil, même si l'initialisation de session a déjà eu lieu.
Le rafraîchissement silencieux de session reste unique par processus.
Cette restauration couvre la recréation d'activité dans le même processus,
pas la destruction du processus par Android.

## Administration et profil personnel

L'accueil présente un seul accès **Administration**, visible et exécutable
uniquement pour le rôle exact `Administrator`. `SessionStatusViewModel.RefreshAsync`
lit les rôles dans `GET account/me` après le rafraîchissement de session ;
une erreur de lecture masque l'accès et affiche une erreur dans la bannière.
La déconnexion invalide aussi les vérifications de droits encore en vol.

`AdministrationPage` regroupe sept onglets : utilisateurs, usages globaux,
rôles, clients OIDC, scopes API, courriels et données. Les en-têtes se
répartissent sur plusieurs lignes sur les écrans étroits.
Le détail d'un utilisateur associe son usage par `UserId`, sans dépendre
du nom affiché. Les erreurs ne déclenchent pas de repli sur l'usage personnel.
Le backend réserve aussi les listes aux administrateurs, sous la
politique `AdministratorOnly`. Le rôle `Moderator` seul n'ouvre plus
l'administration de PostIt. L'autorisation serveur reste indispensable,
indépendamment de la visibilité du bouton dans PostIt.

**Mon profil** réutilise la page de gestion du compte existante et les
endpoints `account/me` pour lire et enregistrer les informations personnelles.
Elle affiche aussi `resource-usage/me`, sans appeler la liste des usages
globaux. Une indisponibilité de l'usage n'empêche pas la gestion du profil.

### Outils d'administration du site

Les outils portés correspondent aux contrôleurs de
`Yavsc.Org/Controllers/Administration` ; le site web existant est conservé.
Les nouvelles routes vivent dans Yavsc.Api sous `api/v1/administration`.
Les données passent par des records dédiés de Yavsc.Abstract et par
`AdministrationApiClient`, jamais par les entités EF.

| Routes | Fonction |
| --- | --- |
| `summary`, `roles`, `roles/ensure` | Statistiques, informations du runtime, rôles standards |
| `roles/{id}/members[/{userId}]` | Membres, inscription et exclusion |
| `clients[/{id}]` | Paramètres scalaires des clients OAuth/OIDC |
| `clients/{id}/collections/{collection}[/{rowId}]` | Redirect URIs, post-logout URIs, scopes, grant types, CORS, restrictions IdP, claims et propriétés |
| `clients/{id}/secrets[/regenerate]`, `clients/{id}/secrets/{secretId}` | Ajout, rotation et suppression des secrets |
| `scopes[/{id}]` | Création, consultation, modification et suppression des scopes API |
| `mailing-criteria`, `mailing-templates[/{id}]` | Critères de diffusion et modèles de courriels |
| `transfer` | Export par GET, import transactionnel par POST |

Les formulaires de clients, scopes et modèles sont construits à partir
des propriétés de leurs DTO typés, afin de conserver tous les champs de
l'interface web. Les booléens et énumérations disposent de contrôles dédiés ;
les valeurs numériques et annotations de validation sont vérifiées avant
l'envoi puis à nouveau côté serveur.

Un nouveau client démarre avec `authorization_code`, PKCE et les scopes
`openid` et `profile`. Les autres types d'autorisation restent configurables
dans ses collections. Une rotation remplace tous les secrets précédents et
expire par défaut après 90 jours. Le serveur conserve uniquement le SHA-256
du secret partagé ; les lectures ne rendent que les métadonnées.
PostIt efface la valeur nouvellement créée au changement de client, à la
prochaine opération et au détachement de la page. Une réponse de création
de secret reçue après déconnexion est ignorée ; un rafraîchissement sans
droits efface les données d'administration précédemment chargées. Supprimer un objet,
exclure un membre, remplacer les secrets ou importer demande une confirmation
explicite. Le serveur refuse l'exclusion du dernier administrateur.

Le transfert JSON version 1 comprend les informations métier des comptes
(identifiant, nom, e-mail, nom complet, avatar, calendrier et limites de
stockage), les billets (contenu, auteur, publication, cercles et étiquettes)
et les paramètres des activités. Ce n'est pas une sauvegarde complète de
la base : fichiers, sessions, mots de passe, jetons, rôles, moyens de paiement,
adresses et historique d'audit ne sont pas transférés. L'e-mail d'un compte
existant, clé alternative du modèle actuel, ne peut pas être changé par import.
Les nouveaux comptes n'ont ni mot de passe ni e-mail confirmé.

Les identifiants des billets sont préservés et la séquence PostgreSQL est
réajustée après l'import. Tous les auteurs et cercles doivent exister et
les parents d'activités doivent former un graphe sans cycle. Une erreur
annule l'ensemble de l'import ; aucun repli n'élargit les permissions d'un
billet. Les fichiers importés et requêtes sont limités à 16 Mio ; les gros
exports doivent être répartis en plusieurs transferts de même schéma.
Si plusieurs étiquettes portent déjà le même nom, l'import réutilise celle
ayant le plus petit identifiant sans supprimer les autres.

L'initialisation du premier administrateur reste le mécanisme web existant,
et n'est pas proposée aux utilisateurs non administrateurs par PostIt.
En développement, `/openapi/administration.json` décrit uniquement ces outils,
avec accès `AdministratorOnly`. Les requêtes de référence sont dans
[Administration.http](../../src/Yavsc.Api/Administration.http) ; l'adresse
locale d'exemple est celle du serveur de tests, à adapter au déploiement.

## Lecture des billets et fichiers utilisateur

La page des blogs affiche par défaut une vue de lecture : titre, login
de l'auteur, date de modification et contenu Markdown dans une zone
défilante de largeur limitée. **Modifier** ouvre l'éditeur existant et
**Nouveau billet** initialise explicitement un brouillon. **Lire** affiche
le contenu enregistré sans effacer les modifications non enregistrées ;
un nouveau billet ne peut pas remplacer un brouillon modifié non sauvé.

La base des ressources est dérivée de `Settings.Authentication.Authority`
(le déploiement Yavsc.Org), jamais de `BlogsApiUrl` :
`https://<hôte Org>/files/<Author.UserName>/`. Le repli sur `AuthorId`
conserve la compatibilité avec les anciens DTO dépourvus d'auteur détaillé.
Un lien `photo.jpg` ou `blogs/42/video.mp4` est relatif à ce répertoire ;
un lien `/files/paul/photo.jpg` conserve son chemin depuis la racine Org.
Les liens absolus ne sont pas réécrits. Les images Markdown `![texte](url)`
sont affichées dans le billet et leur résolveur n'accepte que HTTP(S),
sans accès aux fichiers locaux du poste.
Le résolveur charge chaque image dans un flux mémoire repositionnable
initialement à la position zéro, nécessaire au décodage du lecteur Markdown.

Le lecteur utilise `Markdown.Avalonia.Tight` 12.0.0-a3 (version compatible
Avalonia 12) pour déléguer les hyperliens à une commande PostIt. L'éditeur
conserve `MarkdownViewer.Core` : son gestionnaire intégré ouvre le navigateur
avant d'émettre l'événement de lien et ne permet donc pas ce routage.

Au clic sur un fichier sous `/files/<login>/...` de la même origine Org
(schéma, hôte et port), `BlogResourceService` sonde `Content-Type` avec
`HEAD`, puis un `GET` limité par `Range: bytes=0-0` si le serveur refuse
`HEAD` (405/501). Le corps n'est pas téléchargé pour la détection. Une
extension connue sert de repli uniquement en l'absence de type exploitable
(type absent ou `application/octet-stream`). Les erreurs HTTP/réseau sont
affichées dans la barre de statut, sans les masquer par une déduction MIME.

Les types `video/*` ouvrent `VideoPage` via `App.PushPageAsync` sur desktop.
Cette page contient un `LibVLCSharp.Avalonia.VideoView`, lecture/pause et
une action d'ouverture externe. Le lecteur et ses ressources natives sont
arrêtés et libérés à la sortie de la page. La lecture utilise directement
`new LibVLC()`, `new MediaPlayer(libVlc)` et `Play(media)`, sans options
spécifiques à Linux ni détection VAAPI/VDPAU. Le média est libéré après
`Play` comme dans l'exemple officiel ; le lecteur conserve sa référence native.
**LibVLC doit être installé**
sur le poste (par exemple `libvlc5` et les plugins VLC sur Linux ; les
bibliothèques natives adaptées sont nécessaires sous Windows/macOS).
Une absence de LibVLC ou une erreur de décodage est signalée dans la page.
Le navigateur ne s'ouvre que sur demande explicite, jamais en repli automatique.
Après la navigation, `TryUpdateNativeControlPosition()` synchronise la surface
native à la priorité `Loaded`, une fois la mise en page effectuée :
les transformations de transition ne déclenchent pas de changement de `Bounds`.
Le rappel est ignoré si le lecteur a été libéré ou remplacé entre-temps.
Sur Android et Browser, où ce contrôle natif desktop n'est pas disponible,
les vidéos sont ouvertes dans le navigateur, comme les autres fichiers
(images liées, PDF, téléchargements) et les liens externes.

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

## Podcasts : capture, direct et replay

`PodcastsPageViewModel` est accessible depuis l'accueil Desktop Windows/Linux.
`CameraStreamer` detecte les cameras DirectShow/V4L2 et les microphones
(DirectShow sous Windows, source PulseAudio par defaut sous Linux).
`PodcastBroadcaster` utilise uniquement LibVLC : encodage H.264/AAC, sortie
`livehttp`, segments MPEG-TS d'environ deux secondes. Un segment n'est
envoye que lorsque l'index local LibVLC le declare complet.

`PodcastApiClient` reutilise le transport bearer et son rafraichissement
des jetons. Les PUT sont numerotes a partir de zero ; une nouvelle tentative
doit avoir exactement les memes octets et la meme duree. Le client supprime
un segment local seulement apres acquittement, et envoie le dernier segment
ferme par LibVLC avant de terminer le direct. La navigation et la deconnexion
arretent la camera. Les erreurs restent visibles ; les fichiers non envoyes
restent disponibles localement pour recuperation manuelle.

`ILiveProcessor` est un singleton dans Yavsc.Api. Il stocke les metadonnees
et empreintes SHA-256 dans un index JSON remplace atomiquement, avec des
segments immuables accessibles uniquement via les endpoints authentifies.
Deux receptions au plus sont tamponnees en parallele ; les validations de
sequence, de proprietaire et de quota sont serialisees avant commit.
Un index ne reference jamais un segment en cours d'ecriture.

| Route sous `api/v1/podcasts` | Role |
| --- | --- |
| `GET /`, `POST /` | Catalogue ou creation d'une emission |
| `GET /{id}` | Metadonnees et segments ordonnes |
| `PUT /{id}/segments/{sequence}?duration=...` | Ingestion `video/mp2t`, auteur uniquement |
| `POST /{id}/complete` | Finalisation idempotente du replay |
| `GET /{id}/index.m3u8` | Playlist HLS EVENT, ENDLIST apres fin/interruption |
| `GET /{id}/segments/{sequence}.ts` | Segment engage, lecture authentifiee |
| `DELETE /{id}` | Suppression d'une emission terminee, auteur uniquement |

Toutes les routes requierent la politique `ApiScope`. La lecture est ouverte
a tous les utilisateurs authentifies avec ce scope, pas seulement a l'auteur.
`PodcastMediaInput` alimente LibVLC en octets MPEG-TS via les requetes
authentifiees existantes, sans transmettre de jeton dans une URL ou un
processus externe. Les rappels natifs ne laissent echapper aucune exception :
l'erreur est remontee a la page de lecture.

Un direct inactif devient `Interrupted` lors de la prochaine lecture de
l'index ; au redemarrage, tous les anciens directs sont interrompus.
Les segments engages constituent le replay, sans transcodage serveur.
Le stockage exige un seul processus par racine et les quotas podcast sont
independants de ceux des fichiers utilisateur. Voir
[INSTALL.md](../../INSTALL.md#podcast-live-broadcasts-and-replays) pour
les limites et le deploiement ; [Podcast.http](../../src/Yavsc.Api/Podcast.http)
pour les requetes.
Le document OpenAPI dedie est `/openapi/podcasts.json`, accessible en
developpement aux administrateurs comme les autres documents de l'API.

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

## Défilement de la page des billets

La page `BlogsPage` utilise une grille à hauteur contrainte : l'en-tête
et les commandes restent visibles, tandis que la liste des billets et
l'éditeur se partagent l'espace disponible. Un `GridSplitter` permet
de modifier cette répartition. La liste et l'article défilent séparément.

Lorsque la largeur de la fenêtre atteint ou dépasse sa hauteur, le lecteur
ou l'éditeur passe à gauche (deux tiers de la largeur par défaut), et
l'en-tête, la recherche, les commandes et la liste passent à droite.
Le séparateur devient vertical. En portrait, la disposition verticale
revient sans recréer les contrôles ni perdre le brouillon.
Le seuil utilise les dimensions clientes de la fenêtre, indépendamment
de la hauteur occupée par la navigation et la bannière de session.

La page applique `NavigationPage.BarHeightOverride` à 36 en mode large ;
le `NavRoot` de `MainView` conserve ainsi le bouton Retour, sans changer
les autres pages. En portrait, la hauteur de navigation par défaut revient.
Les commandes ont un contexte de données explicite pour conserver leurs
liaisons, y compris lorsqu'elles passent dans le menu de débordement.

Dans `MarkdownEditorControl`, AvaloniaEdit gère son propre défilement ;
l'aperçu Markdown est placé dans un `ScrollViewer`. Ne pas limiter
la hauteur du `MarkdownViewer` lui-même : son contenu doit pouvoir
dépasser le viewport pour que les barres automatiques s'activent.
Ne pas placer cette grille dans un `StackPanel` vertical ou un
`ScrollViewer` de page, qui lui donnerait une hauteur de mesure illimitée.

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
