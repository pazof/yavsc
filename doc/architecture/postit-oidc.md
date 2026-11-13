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

1. Au démarrage, PostIt restaure la session silencieusement. Si cette
   reprise échoue, il lance directement le login OIDC, sans confirmation.
   Une nouvelle tentative peut aussi être lancée avec **Se connecter**
   dans la modale d'authentification.
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
| `ViewModels/SessionStatusViewModel` | État partagé de session, démarrage automatique, login/logout et liens de compte. |
| `Views/Layout/MainView`        | Modale d'authentification superposée à la navigation, sans barre d'état persistante. |

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

La modale affiche l'attente du navigateur et les erreurs de connexion.
Les actions sont désactivées pendant le login pour éviter les ouvertures
concurrentes. Le client expose aussi les phases suivantes à ses appelants :

| Phase              | Signification                                                    |
|--------------------|------------------------------------------------------------------|
| `Discovering`      | Fetch de `/.well-known/openid-configuration`.                    |
| `OpeningBrowser`   | Navigateur système ouvert, on attend que l'utilisateur revienne. |
| `AwaitingCallback` | (idem `OpeningBrowser` aujourd'hui — couvre la fenêtre pipe).    |
| `ExchangingCode`   | Trade du `code` contre les tokens à `/connect/token`.            |
| `Success` / `Error`| Issue du flow.                                                   |

Si le flow reste bloqué sur `OpeningBrowser`, **l'OS n'a jamais
relancé PostIt avec l'URL `postit://callback`** : le scheme
handler n'est pas enregistré correctement.

## Persistance et reprise au démarrage

Au boot, `App.OnFrameworkInitializationCompleted` :

1. Construit le `TokenStore` (~/.config/PostIt/tokens.json) et
   `YavscApiClient`. Ce dernier charge `_tokens = store.Load()`
   dans son constructeur.
2. Crée la racine `MainView` et démarre la maintenance de session.
3. À l'initialisation de la racine, appelle `TrySilentLoginAsync` une
   seule fois par processus :
   - si l'access token a encore `> RefreshSkew` (60 s) de vie →
     `Success` immédiat.
   - sinon, tente un refresh via le refresh token. Si l'IdP
     accepte → la session est restaurée. Un rejet permanent
     (`invalid_grant` / `invalid_client`) purge le store ; une erreur
     temporaire conserve les jetons pour une nouvelle tentative.
   - sans session restaurée, lance immédiatement le login interactif.
4. Initialise la messagerie et affiche `HomePage` si la pile est vide.

La maintenance renouvelle les jetons 60 secondes avant expiration,
sans attendre un appel API. Les échecs temporaires sont retentés après
30 secondes ; la reprise d'activité réveille également la maintenance.
L'option Android de maintien en arrière-plan conserve son comportement.

Un échec de login ou de renouvellement, l'expiration sans refresh token,
une déconnexion explicite ou un 401 authentifié persistant après la
tentative de refresh affiche la modale de `MainView`. La navigation en
dessous est désactivée et le clavier reste dans la modale. Celle-ci
défile sur les petits écrans et propose :

- **Se connecter** : nouveau flow OIDC/PKCE ;
- **Créer un compte** : navigateur sur `Account/Register` de l'autorité ;
- **Mot de passe oublié** : navigateur sur `Account/ForgotPassword`
  de la même autorité configurée.

Les liens conservent l'éventuel préfixe de chemin de l'autorité.
L'ouverture d'un lien de compte ne valide pas une session : l'utilisateur
revient ensuite sur **Se connecter**. Un renouvellement silencieux réussi
ferme la modale sans changer la page ni les brouillons.

**Paramètres**, **À propos** et **Se déconnecter** sont dans le menu
secondaire de l'accueil. La déconnexion purge le store puis ramène la
navigation à l'accueil ; elle ne relance pas le navigateur automatiquement.

## Garanties testées

- `LoginInteractiveAsync` émet `Discovering → OpeningBrowser →
  ExchangingCode → Success` (ou `Error` si pas de navigateur).
- `TrySilentLoginAsync` retourne `false` si pas de bundle sur
  disque, `true` si l'access est encore valide, `true` après un
  refresh réussi.
- `SchemeUrlDetector.FindCallbackUrl` reconnaît l'URL en argv,
  case-insensitive, refuse les faux positifs (`postit-…://`),
  retourne le premier match de manière déterministe.

- Démarrage sans session, avec session valide et après refresh silencieux.
- Rejet du refresh token, erreur temporaire puis reprise, expiration sans
  refresh token, refus 401 persistant et déconnexion explicite.
- Modale en portrait compact et paysage bas : actions accessibles par
  défilement, navigation bloquée, restauration du focus et de la page.
- Liens de création de compte et de récupération du mot de passe sur
  l'autorité configurée, y compris avec un préfixe de chemin.

## Voir aussi

- [Architecture.md](../Architecture.md) — racine.
