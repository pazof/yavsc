# PostIt — Client desktop et authentification OIDC

> **Récapitulatif** : PostIt est un client desktop Avalonia 12
> multi-cible (Desktop, Android, Browser/WASM) qui consomme l'API
> Yavsc.Org. Authentification **public client + PKCE** via
> **custom URI scheme** (RFC 8252 §7.1) — pas de loopback HTTP.
> Détail dans cette page, racine de l'architecture : [Architecture.md](../Architecture.md).

## Rôle

PostIt est un client desktop (Avalonia 12, multi-cible : Desktop
Linux/Windows, Android, Browser/WASM) qui consomme l'API
Yavsc.Org pour les opérations CRUD du blog et d'autres
ressources de l'utilisateur. C'est un **public client** : pas de
secret, le secret est remplacé par PKCE.

## Flow d'authentification

L'authentification suit **RFC 8252 §7.1** (OAuth 2.0 for Native
Apps — Custom URI Scheme Redirect), pas le pattern loopback
historique. La séquence :

1. L'utilisateur clique **Se connecter** sur `LoginPage`.
2. `YavscApiClient.LoginInteractiveAsync` demande à `OidcClient`
   de calculer l'authorize URL et de la passer à
   `CustomSchemeBrowser.InvokeAsync`.
3. `CustomSchemeBrowser` ouvre le navigateur système sur l'authorize
   URL via `Process.Start(options.StartUrl) { UseShellExecute = true }`,
   puis **attend** sur un `TaskCompletionSource<string>` alimenté
   par un named pipe `PostIt.OidcCallback`.
4. L'utilisateur s'authentifie sur l'IdP. Le navigateur redirige
   vers `postit://callback?code=***&state=…`.
5. L'OS, qui a le scheme `postit://` enregistré, **lance une 2ᵉ
   instance** de PostIt avec l'URL en argument.
6. La 2ᵉ instance lit `Environment.GetCommandLineArgs()` *avant*
   qu'Avalonia ne boote (`PostIt.Desktop.Program.Main`),
   détecte l'URL via `SchemeUrlDetector.FindCallbackUrl`, l'écrit
   sur le named pipe puis sort par `Environment.Exit(0)`. Aucune
   fenêtre n'est créée.
7. L'instance vivante reçoit l'URL via le pipe, complète la TCS,
   `OidcClient` échange le code contre les tokens (access +
   refresh + id) et les persiste dans `~/.config/PostIt/tokens.json`.

## Pourquoi le scheme custom plutôt que loopback

Le loopback (`http://127.0.0.1:7890/callback`) oblige l'app à
ouvrir un port TCP, à le publier comme redirect URI dans l'IdP,
et à gérer la course "le navigateur revient-il à temps ?".
Le scheme custom transfère la livraison du callback à l'OS :
le navigateur tape sur une URL que l'OS sait router vers PostIt,
pas vers un serveur HTTP.

## Composants partagés (`PostIt/`)

| Composant                       | Rôle                                                              |
|---------------------------------|-------------------------------------------------------------------|
| `Services/OidcLoginPhase`       | Enum des étapes du flow : `Idle / Discovering / OpeningBrowser / AwaitingCallback / ExchangingCode / Success / Error` |
| `Services/YavscApiClient`       | Client HTTP de l'API Yavsc. Porte `LoginInteractiveAsync(IProgress<OidcLoginPhase>)` et `TrySilentLoginAsync`. Refresh silencieux sur 401 et sur access-token bientôt expiré. |
| `Services/BlogApiClient`        | Mapper DTO↔path pour la sous-API blog. **Note** : `pathPrefix` est *relatif* à `/api/v1/` (que porte déjà `BaseAddress`) — ex. `"blog"` pour matcher `[Route(APIPrefix + "/blog")]`. Ne pas ré-inclure `api/`. |
| `Services/SingleInstance`       | Named-pipe helper. `TryHandOffAsync` côté 2ᵉ instance, `StartServerAsync` côté instance vivante. |
| `Services/CustomSchemeBrowser`  | `IBrowser` OidcClient qui ouvre le système + attend le pipe.      |
| `Services/SchemeUrlDetector`    | Détection pure, testable, du `postit://callback` dans argv.       |
| `Services/TokenStore`           | Persiste `RefreshTokenRecord` (access/refresh/id + expiry). Mode 0600 sur POSIX. |
| `ViewModels/SessionStatusViewModel` | VM du bandeau persistant Connecté/Déconnecté + Logout.         |
| `Views/SessionStatusBanner`     | Bandeau affiché en haut de `MainWindow`, visible sur toutes les pages. |

## Plateformes (`PostIt.Desktop`, `PostIt.Android`, `PostIt.Browser`)

Chaque plateforme injecte son propre `IBrowser` via
`Platform.CreateBrowser` :

- **Desktop** : `CustomSchemeBrowser` + scheme `postit://`.
- **Android** : Chrome Custom Tabs via `AndroidSystemBrowser`,
  scheme `android://postit-signin` (déclaré comme
  `<activity-alias>` dans `AndroidManifest.xml`). MainActivity est
  `SingleTask` → `OnNewIntent` livre l'URL à `AndroidOidcCallbackSink`.
- **Browser (WASM)** : pas de process distinct → N/A.

## UX observable

`LoginPage` affiche en continu la **phase** courante du flow
(badge `PhaseLabel`) en plus du `StatusMessage` textuel :

| Phase              | Signification                                                    |
|--------------------|------------------------------------------------------------------|
| `Discovering`      | Fetch de `/.well-known/openid-configuration`.                    |
| `OpeningBrowser`   | Navigateur système ouvert, on attend que l'utilisateur revienne. |
| `AwaitingCallback` | (idem `OpeningBrowser` aujourd'hui — couvre la fenêtre pipe).    |
| `ExchangingCode`   | Trade du `code` contre les tokens à `/connect/token`.            |
| `Success` / `Error`| Issue du flow.                                                   |

Si le badge reste bloqué sur `OpeningBrowser`, **l'OS n'a jamais
relancé PostIt avec l'URL `postit://callback`** : le scheme
handler n'est pas enregistré correctement.

## Persistance et reprise au démarrage

Au boot, `App.OnFrameworkInitializationCompleted` :

1. Construit le `TokenStore` (~/.config/PostIt/tokens.json) et
   `YavscApiClient`. Ce dernier charge `_tokens = store.Load()`
   dans son constructeur.
2. Ouvre `MainWindow` avec `HomePage` comme racine.
3. Sur l'événement `Opened`, appelle `TrySilentLoginAsync` :
   - si l'access token a encore `> RefreshSkew` (60 s) de vie →
     `Success` immédiat, push `MainPage`.
   - sinon, tente un refresh via le refresh token. Si l'IdP
     accepte → push `MainPage`. Si l'IdP rejette (révocation,
     vol, expiration du refresh) → `_store.Clear()`, retour à
     `HomePage`.

Le bandeau `SessionStatusBanner` reflète `Api.HasValidSession`
en continu et expose le bouton **Se déconnecter**, qui appelle
`Api.LogoutAsync()` (purge du store) puis `PopToRootAsync`
pour ramener sur `HomePage`.

## Garanties testées

- `LoginInteractiveAsync` émet `Discovering → OpeningBrowser →
  ExchangingCode → Success` (ou `Error` si pas de navigateur).
- `TrySilentLoginAsync` retourne `false` si pas de bundle sur
  disque, `true` si l'access est encore valide, `true` après un
  refresh réussi.
- `SchemeUrlDetector.FindCallbackUrl` reconnaît l'URL en argv,
  case-insensitive, refuse les faux positifs (`postit-…://`),
  retourne le premier match de manière déterministe.

Le chemin "refresh token rejeté → purge du store" est testé
manuellement ; le stub `OidcStubAuthority` ne sait pas encore
rejeter un refresh token précis — extension future.

## Voir aussi

- [Architecture.md](../Architecture.md) — racine.
