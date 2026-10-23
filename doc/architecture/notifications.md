# Notifications Yavsc : serveur, Web et PostIt

Ce document décrit les mécanismes de notification actuellement présents
dans le dépôt. Le terme « notification » recouvre plusieurs flux distincts :

1. les **annonces persistées** en base, affichées dans le site Web et sur
   l'accueil de PostIt ;
2. les **messages temporaires de l'interface Web**, attachés à une réponse
   MVC par `ViewBag.Notify` ;
3. les **événements temps réel** du chat et les événements métier diffusés par
   SignalR.

Ces flux n'ont ni le même stockage, ni le même cycle de vie, ni les mêmes
clients.

## Vue d'ensemble

| Flux | Serveur / transport | Web | PostIt |
|---|---|---|---|
| Annonce persistée | Entité `Notification` en base, API HTTP | Affichée dans le layout et masquable | Liste sur `HomePage`, chargée par HTTP et masquable |
| Message d'interface | Liste de portée requête dans `ViewBag.Notify` | Affichée comme alerte dans le layout | Non consommé |
| Événement de chat | Hub SignalR authentifié à `/chathub` | Le site Web n'en consomme pas dans ses vues MVC | `notifyUser` et `notifyRoom` alimentent les journaux du chat |
| Événement métier `push` | SignalR vers les connexions actives, plus e-mail | Pas de consommateur identifié dans les vues Web | Pas de gestionnaire `push` dans le client PostIt actuel |

Les notifications persistées sont donc différentes d'une notification système
native (notification du navigateur ou du système d'exploitation). Le client
PostIt affiche une liste dans sa page d'accueil ; il ne demande pas de
permission de notification native.

## 1. Annonces persistées

### Modèle et stockage

L'entité partagée [`Notification`](../../src/Yavsc.Abstract/Messaging/Notification.cs)
est enregistrée dans la table `Notification` via `ApplicationDbContext`.
Elle contient :

- `Id`, identifiant généré ;
- `title` et `body`, obligatoires, limités respectivement à 1 024 et 512
  caractères ;
- `icon`, `sound`, `tag`, `color`, propriétés optionnelles ;
- `click_action`, obligatoire, utilisé comme libellé d'action dans les
  interfaces ;
- `Target`, chaîne de ciblage. Les valeurs documentées par le modèle sont
  `null` (tout le monde), `user/{UserId}`, `pub/{topic}` et
  `administration`.

Le masquage d'une annonce pour un utilisateur connecté est stocké dans
`DismissClicked`, avec une clé composée de l'identifiant utilisateur et de
l'identifiant de notification. Le même avis peut ainsi être masqué
indépendamment par chaque utilisateur.

### API

[`NotificationsApiController`](../../src/Yavsc.Api/Controllers/Administration/NotificationsApiController.cs)
expose les deux routes GET suivantes, sous le préfixe `/api/v1` :

| Route | Accès | Résultat |
|---|---|---|
| `GET /api/v1/notifications` | Anonyme ou authentifié | Notifications visibles à l'appelant, non masquées |
| `GET /api/v1/notifications/me` | Anonyme ou authentifié | Même action et mêmes règles que la route sans suffixe |

La réponse est une liste JSON triée par `Id` décroissant. L'API exclut les
notifications masquées par l'utilisateur authentifié, puis applique les
règles d'audience : cible vide, cible `pub/…`, cible correspondant à
`user/{identifiant courant}`, ou cible `administration` pour un administrateur.
Une requête anonyme n'a ni identifiant utilisateur ni rôle administrateur.

[`DismissClicksApiController`](../../src/Yavsc.Api/Controllers/Administration/DismissClicksApiController.cs)
est réservé aux utilisateurs authentifiés :

| Route | Effet |
|---|---|
| `GET /api/v1/dismiss` | Liste les identifiants masqués par l'utilisateur |
| `POST /api/v1/dismiss/click/{notificationId}` | Masque l'avis pour l'utilisateur ; l'opération est idempotente |
| `DELETE /api/v1/dismiss/click/{notificationId}` | Rétablit l'avis s'il était masqué ; renvoie `404` sinon |

Le POST renvoie `404` si l'identifiant de notification n'existe pas. Le modèle
de réponse est [`DismissedNotification`](../../src/Yavsc.Abstract/Messaging/DismissedNotification.cs).

### Création et notifications métier

Le contrôleur MVC
[`NotificationsController`](../../src/Yavsc.Org/Controllers/Communicating/NotificationsController.cs)
permet la gestion CRUD des enregistrements de notification.

Le service [`YavscMessageSender`](../../src/Yavsc.Server/Services/YavscMessageSender.cs)
crée aussi une notification persistée, ciblée sur `user/{id}`, pour les
événements `RdvQueryEvent` et `ClientEstimateSignedEvent`. L'enregistrement
est effectué avant l'envoi d'e-mail. Le même service tente ensuite d'envoyer
un e-mail si l'adresse est disponible et confirmée, puis diffuse l'événement
`push` aux connexions SignalR actives de l'utilisateur.

Cette diffusion `push` est conditionnée par la présence de connexions actives ;
l'avis stocké en base reste, lui, disponible lors d'une récupération ultérieure
par l'API.

## 2. Site Web

### Annonces persistées sur l'accueil

[`HomeController.Index`](../../src/Yavsc.Org/Controllers/HomeController.cs)
charge les notifications non masquées et les place dans `ViewBag.Notify`.
[`_Layout.cshtml`](../../src/Yavsc.Org/Views/Shared/_Layout.cshtml) les rend
comme des alertes Bootstrap avec icône, titre, corps et action. Le titre et le
corps passent par les composants Markdown du site.

Le clic sur l'action appelle `POST /Home/DismissNotification/{id}` via
[`site.js`](../../src/Yavsc.Org/wwwroot/js/site.js) :

- pour un utilisateur connecté, le masquage est enregistré dans
  `DismissClicked` ;
- pour un visiteur anonyme, les identifiants sont conservés dans la session
  Web (`clicked`).

La route renvoie `404` si l'avis n'existe pas et `204` après le masquage.
Comme l'API, l'accueil Web filtre `Target` : les cibles `user/{id}` ne sont
visibles qu'à l'utilisateur correspondant, et la cible `administration`
uniquement aux administrateurs. Un visiteur anonyme ne reçoit que les
notifications publiques (`Target` vide ou préfixe `pub/`).

### Messages temporaires par requête

Les helpers [`NotifyInfo`, `NotifyWarning` et `Notify`](../../src/Yavsc.Org/Helpers/ControllerHelpers.cs)
ajoutent des objets à `ViewBag.Notify`. Ce mécanisme n'écrit pas en base :
il ne vit que dans la réponse MVC qui rend le layout. Les méthodes
`NotifyInfo` et `NotifyWarning` partagent actuellement le même rendu générique
`alert-info`.

## 3. PostIt

[`NotificationsApiClient`](../../src/Yavsc.Api.Client/NotificationsApiClient.cs)
construit les routes à partir de la base d'API de l'hôte métier configuré.
Il fournit les méthodes de lecture publique et authentifiée, ainsi que les
opérations de lecture, masquage et restauration.

Sur [`HomePage`](../../src/PostIt/PostIt/Views/Layout/HomePage.axaml),
[`HomePageViewModel`](../../src/PostIt/PostIt/ViewModels/Layout/HomePageViewModel.cs)
charge la liste :

- sans session valide, il appelle la route publique
  `GET /api/v1/notifications` ;
- avec une session valide, il appelle
  `GET /api/v1/notifications/me` avec le client HTTP authentifié.

Le chargement est lancé à l'ouverture de la page et relancé lorsque l'état de
connexion change. Un compteur de version ignore les réponses obsolètes si un
chargement plus récent a déjà commencé. L'interface distingue l'état de
chargement, la liste vide et l'erreur de chargement ; l'utilisateur peut aussi
actualiser manuellement.

Le bouton d'une notification utilise `click_action` comme libellé et masque
l'élément. En session authentifiée, PostIt appelle
`POST /api/v1/dismiss/click/{id}` avant de retirer l'élément de la liste. Si
l'appel échoue, l'élément reste affiché et une erreur est présentée. Sans
session, l'élément est retiré de la liste locale, sans persistance du
masquage côté serveur.

## 4. Événements temps réel

### Hub du chat

Le hub [`ChatHub`](../../src/Yavsc.Server/Hubs/ChatHub.cs) est publié par
Yavsc.Api à la racine sous `/chathub` (et non sous `/api/v1`) et porte
`[Authorize]`. Il s'appuie sur SignalR et sur `IConnexionManager` pour suivre
les utilisateurs et les salons.

Les événements de notification émis par le hub comprennent notamment :

| Événement SignalR | Données | Utilisation PostIt |
|---|---|---|
| `notifyUser` | `type`, identifiant ou nom cible, message | Ajout au journal système |
| `notifyRoom` | `type`, salon, utilisateur ou détail | Ajout au journal du salon ; met à jour la liste d'utilisateurs sur `UserJoin` |
| `joint` | `ChatRoomInfo` | Initialise la vue du salon rejoint |
| `ReceiveMessage` | message avec nom, salon et corps | Ajoute le message à l'historique du salon |
| `notifyUserInRoom` | `type`, salon, message | Pas de gestionnaire correspondant dans `ChatViewModel` actuellement |

Les constantes de types du chat sont déclarées dans
[`NotificationTypes`](../../src/Yavsc.Abstract/Messaging/NotificationTypes.cs).
Les messages `notifyUser` et `notifyRoom` ne sont pas enregistrés en base :
ils décrivent l'état temps réel de connexion, de salon ou les erreurs de
conversation.

Le [`ChatViewModel`](../../src/PostIt/PostIt/ViewModels/Chat/ChatViewModel.cs)
ouvre le hub avec le jeton d'accès PostIt, tente un renouvellement silencieux
du jeton, et active la reconnexion automatique. Après reconnexion, il rejoint
à nouveau le salon sélectionné, car la connexion SignalR précédente et ses
groupes ne sont plus actifs. Les callbacks mettent à jour les collections sur
le thread UI d'Avalonia.

### Événement métier `push`

En plus des événements du chat, `YavscMessageSender` utilise le même contexte
SignalR pour émettre `push` avec le topic et une charge utile sérialisée. Ce
nom d'événement est distinct de `notifyUser` et `notifyRoom`. Aucun
gestionnaire `push` n'est présent dans le client PostIt actuel : les annonces
métier persistées sont visibles dans PostIt lors du prochain chargement ou
rafraîchissement HTTP.

## 5. Points d'attention pour les évolutions

- Conserver les règles d'audience alignées entre l'API et le site Web ; en
  particulier, appliquer un filtrage `Target` côté Web avant d'exposer les
  avis dans le layout.
- Ne pas confondre masquage utilisateur (`DismissClicked`) et suppression
  globale de la notification : le masquage API est par utilisateur.
- Les sessions anonymes du site Web et PostIt n'ont pas le même comportement :
  le site Web conserve le masquage en session, tandis que PostIt ne le garde
  que dans la liste affichée jusqu'au prochain rechargement.
- Ajouter un nouveau type de cible ou un nouvel événement temps réel implique
  d'examiner séparément son filtrage, son transport et les clients qui
  l'écoutent.
- Les références à GCM et le nom historique de `YavscMessageSender` ne
  signifient pas que PostIt reçoit une notification native hors ligne : le
  chemin observé ici est l'e-mail, l'événement SignalR temps réel et la
  notification persistée récupérée par HTTP.

## Références d'implémentation

- [`Notification`](../../src/Yavsc.Abstract/Messaging/Notification.cs)
- [`NotificationsApiController`](../../src/Yavsc.Api/Controllers/Administration/NotificationsApiController.cs)
- [`DismissClicksApiController`](../../src/Yavsc.Api/Controllers/Administration/DismissClicksApiController.cs)
- [`NotificationsController`](../../src/Yavsc.Org/Controllers/Communicating/NotificationsController.cs)
- [`HomeController`](../../src/Yavsc.Org/Controllers/HomeController.cs)
- [`YavscMessageSender`](../../src/Yavsc.Server/Services/YavscMessageSender.cs)
- [`ChatHub`](../../src/Yavsc.Server/Hubs/ChatHub.cs)
- [`NotificationsApiClient`](../../src/Yavsc.Api.Client/NotificationsApiClient.cs)
- [`HomePageViewModel`](../../src/PostIt/PostIt/ViewModels/Layout/HomePageViewModel.cs)
- [`ChatViewModel`](../../src/PostIt/PostIt/ViewModels/Chat/ChatViewModel.cs)
