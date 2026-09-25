# Changelog

## [1.0.8-rc17] - unstable

Fichiers utilisateur et pièces jointes sur les devis/demandes (PostIt +
API), système de génération `contrib` (vhosts nginx HTTP/3 edge +
appsettings jq + QUIC logiciel Kestrel), et correction du bug de
production où `IsPublished` disparaissait du payload de liste du blog API.

### Added

* [PostIt] Espace de stockage personnel (« MyFiles ») : page de gestion
  des fichiers de l'utilisateur, entrée depuis la page d'accueil.
* [PostIt] Pièces jointes aux devis et aux demandes : attachement depuis
  le détail de demande et la page d'édition de devis.
* [Yavsc.Api] Endpoints d'attachement de fichiers sur les devis
  (`EstimateApiController`) et les demandes front-office
  (`FrontOfficeApiController`).
* [Yavsc.Api.Client] `UserFilesApiClient` + DTOs associés ; méthodes
  d'attachement sur `EstimateApiClient` et `FrontOfficeApiClient`.
* [Yavsc.Server] Modèles `EstimateAttachedFile`, `QueryAttachedFile`,
  `BlogAttachedFile`, `AttachFileRequest` ; migration EF Core
  `UserFileAttachments`.
* [contrib] Système de génération `Makefile` : vhosts nginx HTTP/3 edge
  (`template.vhost`, `QUIC_OPTS`), appsettings générés via jq
  (`template.appsettings-{org,api,blogs}.json`, échappement JSON-safe des
  secrets), QUIC logiciel Kestrel (`Http1AndHttp2AndHttp3`), et
  `README.nginx.md`.
* [Tests] Projet `Yavsc.Server.Tests` : tests du service
  `BlogSpotService.Details` + `ServerServicesFixture` (SQLite `:memory:`
  par défaut, Npgsql opt-in via `YAVSC_SERVER_TEST_DB_PROVIDER`).
* [Tests] `IBlogPostWireShapeTests` (forme du wire blog),
  `EstimateAttachmentsApiTests`, `FrontOfficeQueryAttachmentsApiTests`,
  `FileSystemApiTests` (pièces jointes et stockage perso).
* [CI] Intégration Gitleaks / analyse de secrets (`.pre-commit-config.yaml`).

### Changed

* [contrib] `QUIC_OPTS` (`reuseport`) désormais activé uniquement en
  production sur le vhost Org. La preprod et la prod partageant la même
  IP, `reuseport` — légal une seule fois par `addr:port` (443/udp) —
  serait sinon en conflit entre preprod-Org et prod-Org et ferait refuser
  la config par nginx.
* [Yavsc.Api.Test] `ApiWebServerFixture` : la connexion admin PostgreSQL
  provient de la variable d'environnement
  `YAVSC_API_TEST_NPGSQL_ADMIN_CONNECTION` ; plus de mot de passe en
  source.

### Fixed

* [Yavsc.Blogs] Bug de production : `IsPublished` était absent du payload
  de la liste du blog API. Le endpoint `GetBlogspot` retourne
  `IEnumerable<IBlogPost>` sérialisé par System.Text.Json d'après le type
  déclaré (l'interface), qui omet les propriétés du type concret —
  `IsPublished` vivait sur `BlogPost` / `BlogPostDto` mais pas sur
  `IBlogPost`. PostIt lisait donc `IsPublished == false` pour tout billet,
  y compris publiés (https://yavsc.pschneider.fr/BlogSpot/Details/13).
  Corrigé en déclarant `IsPublished` sur `IBlogPost`.
* [Yavsc.Org] Accès au Swagger UI : les actions MVC scaffoldées de
  `CommentsController` (Index/Details/…) atterrissaient sur la même route
  `POST api/v1/blogcomments` que l'action d'API `Post`, provoquant une
  `SwaggerGeneratorException` (conflit method/path) au chargement de
  `/swagger`. Corrigé par `[ApiExplorerSettings(IgnoreApi = true)]` sur
  les actions scaffoldées.

## [1.0.8-rc16] - unstable

Flux de signature et de génération de devis : un devis peut désormais être
signé par les deux parties (fournisseur et client) puis téléchargé en PDF
ou TeX depuis PostIt, avec les signatures dessinées en vecteur.

### Added

* [PostIt] Pad de signature sur la page de validation d'un devis : on valide
  ou on refuse en joignant une signature (côté fournisseur ou client), capturée
  au format filaire PostIt (strokes) et envoyée sur l'acceptation.
* [Yavsc.Api] Finalisation de `POST /api/v1/front/query/accept` et `.../reject` :
  autorisation selon le rôle (fournisseur/client/admin), signature optionnelle
  persistée sur l'estimate avec horodatage `ProviderValidationDate` /
  `ClientValidationDate` selon le côté signataire.
* [Yavsc.Api] `QueryStatus.Accepted` éclaté en `ProAccepted` / `ClientAccepted`
  pour distinguer l'acceptation de chaque partie.
* [Yavsc.Api] Endpoints de devis `GET /api/v1/front/query/{id}/estimate.tex` et
  `estimate.pdf` (clé par id de query). Le flux de génération TeX/PDF est
  rapatrié de Yavsc.Org vers Yavsc.Api (`TeXHelpers`, `PdfGenerationViewModel`,
  `BillViewComponent`, modèles Bill/Estimate, `_ViewImports` ; `Program.cs`
  câble `AddControllersWithViews` + `SiteSettings`).
* [Yavsc.Api] Signatures du devis dessinées en TikZ vectoriel
  (`TeXHelpers.SignatureToTikz`) en bas du document, fournisseur et client
  côte à côte dans deux minipages ; plus de dépendance à un PNG de signature.
* [Yavsc.Api.Client] `DownloadAsync` sur le client + `GetEstimateTexAsync` /
  `GetEstimatePdfAsync` ; boutons « Télécharger PDF » / « Télécharger TeX »
  dans PostIt sur la page de validation du devis et le détail de commande,
  avec un helper `StorageProvider` d'enregistrement de fichier.
* [PostIt] La liste des devis se rafraîchit au retour de navigation (pop-back),
  de sorte qu'un devis validé quitte immédiatement la liste du signataire.
* [PostIt] Renommage des titres de liste : « Devis en attente de signature
  fournisseur » / « Devis en attente de signature client ».
* [Yavsc.Abstract] `SignatureType` déplacé dans `Yavsc.Abstract.Billing`.
* [CLI] Option `-c configuration-file.json` au lancement.
* [Tests] `EstimatePdfGenerationTests` : tests unitaires du pipeline
  signature → TikZ → PDF (devis signé des deux côtés), assertant que le PDF
  existe au chemin de sortie attendu et que seul le PDF persiste (aux/log
  nettoyés) ; skip propre si `lualatex` est absent.
* [README] Dépendances système au runtime pour la génération PDF :
  `texlive-binaries` (lualatex), `texlive-luatex` (luaotfload),
  `texlive-pictures` (tikz), `texlive-fonts-extra` (bera).

### Changed

* [Yavsc.Api] `GetOngoingEstimatesAsProvider` filtre désormais sur
  `ProviderValidationDate` : un devis signé par le fournisseur quitte sa liste
  « en attente ».
* [Yavsc.Api] La génération PDF compile via le `lualatex` système, le TeX fourni
  sur l'entrée standard (`/dev/stdin`), le PDF écrit directement dans le
  répertoire des factures (`Site.Bills`), les artefacts `.aux`/`.log` nettoyés
  afin que seul le `.pdf` persiste.

### Fixed

* [Yavsc.Api] `Emergency stop` à la génération PDF : `lualatex` sans argument
  fichier lit stdin en mode terminal et s'arrête après la première ligne d'un
  document multi-lignes. Corrigé en passant `/dev/stdin` comme nom de fichier
  d'entrée — stdin est alors traité comme un fichier régulier et le document
  multi-lignes compile. C'était la cause racine de l'échec en production, pas
  `bera` ni `luaotfload` (problèmes réels mais séparés, comblés côté paquets).
* [Yavsc.Api] `I can't write on file 'estimate-1.log'` : le chemin relatif
  `Site.Bills` (`"bills"`) était imbriqué quand il servait à la fois de
  `-output-directory` et de répertoire de travail ; résolu en chemin absolu.
* [Yavsc.Api] NRE dans `RenderViewToString` (RouteData / `ViewData.Model`
  nuls) : reconstruit sur le `ControllerContext` du contrôleur ; `IViewEngine`
  résolu via `IRazorViewEngine` (.NET 10 n'enregistre pas de `IViewEngine`
  unique).
* [Yavsc.Api] `Layout = "null"` (chaîne) dans les modèles TeX cherchait
  `null.cshtml` ; corrigé en `Layout = null` (null C#).
* [Yavsc.Api] `texi2pdf` (outil Texinfo, inadapté au LaTeX) remplacé par
  `lualatex`.
* [Yavsc.Api] Les diagnostics lualatex étaient perdus (écrits dans le `.log`
  qu'on supprimait) ; stdout/stderr sont désormais capturés dans le message
  d'erreur, et le `.log` est conservé en cas d'échec.

## [1.0.8-rc15] - unstable

### Added

* [PostIt] Saisie et envoi de pieces jointes sur un billet de blog: le client monte les fichiers en `multipart/form-data` lors du `Save`, ajoute un lien Markdown par piece jointe a l'article, puis re-PUT le billet. Une collection `DraftAttachments` porte les fichiers en cours d'edition.
* [PostIt] Apercu Markdown dans l'editeur de blogs (integration `MarkdownViewer.Core`) avec bascule edition/preview sur `BlogsPage`.
* [PostIt] Gestion des devis: page de liste `EstimateListPage` (vues client et prestataire), page d'edition `EstimateEditionPage` avec lignes de devis (`EstimateLineItemViewModel`), et flux de validation d'un devis.
* [PostIt] Flux des requetes en cours cote prestataire (`ProviderOngoingRequestsPage`) avec tri persistant.
* [Yavsc.Api] Endpoints devis `GET /api/v1/estimate/asclient` et `GET /api/v1/estimate/asprovider` pour lister les devis en cours selon le role.
* [Yavsc.Api] Endpoint `GET /api/v1/billing/provider/ongoing` et `POST /api/v1/billing/prosign/{billingCode}/{id}` pour le flux prestataire.
* [Yavsc.Api.Client] `EstimateApiClient` + DTOs (`EstimateDto`, `EstimateLineDto`) pour le pipeline devis cote client lourd.
* [Yavsc.Api.Client] Reprise de l'upload de fichiers blog dans `BlogApiClient` (`CreateMultipartContent`, `BlogUploadFile`) — la meme forme filaire que PostIt utilise en production.
* [Yavsc.Abstract] `FileServerUrlHelpers` pour deriver les URLs publiques des fichiers utilisateur depuis l'autorite OIDC (alignement sur `UserFilesPath`).
* [Yavsc.Server] `BlogSpotService.AttachFiles` : ecrit les pieces jointes sous la racine des fichiers utilisateur (`{UserFilesDirName}/{user}/blogs/{postId}/{fileName}`).
* [Yavsc.Blogs] `BlogApiController` accepte `multipart/form-data` (champ `blog` + parts `file`) sur `POST` et `PUT /api/v1/blogspot`, en plus du payload JSON existant.
* [PostIt] Variable d'environnement `POSTIT_SETTINGS_JSON` pour surcharger les parametres au lancement (utile en CI/test).
* [Yavsc.Server] Migration EF `fileACL`: `CircleAuthorizationToFile` gagne `OwnerId` + `Access` et un graphe proprieetaire, base du controle d'acces fichier par cercle.
* [Yavsc.Server] Migration EF `genericEstimate`: `Estimate.Query` passe de `RdvQuery` a `NominativeServiceCommand?` pour supporter des devis non lies a un RDV.
* [Yavsc.Server] Migrations EF `NominativeServiceCommand` et `AddHairCutQueryLocationId` (localisation de la coupe).
* [CI] Integration du scan de secrets Gitleaks dans le pipeline Forgejo + `.gitleaksignore`.
* [Tests] Non-regressions: round-trip multipart blog (`BlogApiTests`, `CreateMultipartContentTests` forme + serveur), `BlogAttachmentLinkTests`, `FileServerUrlHelpersTests`, `EstimateApiControllerTests`, VMs `EstimateList`/`EstimateEdition`/`ProviderOngoingRequests`, `BillingControllerTests` (filtrage par utilisateur), `GetOpenIdConfiguration` contre l'hote de test.

### Changed

* [PostIt] Renommage `MainPage` -> `BlogsPage` (et `PushMainPageAsync` -> `PushBlogsPageAsync`) pour refletter le role de la page.
* [PostIt] Refacto des `SiteSettings` : reorganisation des parametres, nouvelles valeurs par defaut, et indications plus claires dans les `appsettings`.
* [Yavsc.Api] `BillingController` ne liste plus que les codes billing de l'utilisateur authentifie (filtre par `UserId`), au lieu de l'ensemble des requetes prestataire.
* [Yavsc.Api] Mutualisation d'un `YavscMessageSender` partage dans l'hote API (au lieu d'une instance par controller).
* [Yavsc.Org] La detection du doublon d'email a l'inscription utilise desormais le code erreur generic `DbException` 23505 (fournisseur-agnostique) au lieu de `PostgresException`, pour rester correct en test SQLite.
* [PostIt] `YavscApiClient` tolere les corps vides (`204 No Content` sur `PUT /blogspot/{id}`) au lieu de lever un `JsonException`.
* [CI] Le pipeline compile avant les tests, installe les dependances natives, et retire le `ItemGroup.Using Include=Xunit` superflu.

### Fixed

* [PostIt] Suppression d'un faux message d'erreur affiche apres un `PUT /blogspot/{id}` reussi (204 traite comme succes, plus comme une reponse vide invalide).
* [PostIt] Correction de la creation des liens de pieces jointes (`TryAppendAttachmentLinks`) : segment proprietaire et chemin `blogs/{id}/{fileName}` alignes sur ce que `AttachFiles` ecrit reellement.
* [Yavsc.Blogs] Correction de l'upload / de la persistance des pieces jointes (`FileSystemHelpers` + `BlogApiController` multipart).
* [Tests] Stabilisation des suites: seeds billing avec metadonnees d'audit, `GetOpenIdConfiguration_returns_ok` contre l'hote de test, retraits d'assertions abusives, tests SQLite verts.
* [PostIt] Nettoyage de fichiers temporaires accidentels et ignore des repertoires `tmp` dans le depot.

## [1.0.8-rc14] - unstable

### Added

* [PostIt] Ajout d'un `BillingQueryDetailsPageViewModel` et de sa page associee pour afficher le detail d'une commande billing depuis l'historique.
* [PostIt] Ajout d'un mode detail avec section metier (statut, date, description, motif, infos) et section technique repliable (code, client, provision, lieu, prestations).
* [PostIt] Ajout d'un badge de statut enrichi (couleur + pictogramme) sur le detail d'une commande pour visualiser l'etat en un coup d'oeil.
* [PostIt] Ajout d'un bloc d'actions rapide en tete du detail (`Retour`, `Ouvrir en edition`) pour eviter le scroll jusqu'au bas de page.
* [PostIt] Ajout d'un style monospace sur les metadonnees techniques (code billing, client, provision, lieu, prestations) pour faciliter la lecture des identifiants et valeurs brutes.

### Changed

* [PostIt] Le bouton d'ouverture depuis la liste billing ouvre maintenant une page de detail dediee avant l'eventuelle edition.
* [PostIt] Amelioration UX des pages billing: badges de statut colores, actions remontees en haut de page, et typographie monospace sur les metadonnees techniques.

### Fixed

* [Yavsc.Api] Correction d'un 500 sur le refresh du catalogue d'activites lorsque `Activity.Description` est `NULL` en base (nullabilite explicite + projection null-safe + gardes sur codes vides).
* [Yavsc.Api] Correction des erreurs 400/500 sur les routes billing (`Rdv`, `Brush`, `MBrush`) en imposant `ClientId` depuis l'utilisateur authentifie et en ignorant les champs server-owned lors de la validation modele.
* [Yavsc.Api] Correction du `PUT /api/v1/billing/Rdv/{id}`: mise a jour controlee de l'entite existante (et non remplacement brut du graphe JSON), ce qui supprime les `BadRequest` parasites.
* [Yavsc.Api] Correction PostgreSQL `timestamptz` sur RDV: normalisation UTC de `EventDate` sur `POST/PUT /api/v1/billing/Rdv` pour eviter l'erreur `Cannot write DateTime with Kind=Local`.
* [Yavsc.Api] Correction du flux FrontOffice accept/reject de query: sauvegarde avec contexte utilisateur et fallback d'injection pour `IBillingService` afin d'eviter les erreurs serveur en environnement de test.
* [Yavsc.Blogs] Correction des `BadRequest` sur `POST/PUT /api/v1/blogspot` avec payload JSON (PostIt): les proprietes de navigation/serveur (`Author`, `Tags`, `Comments`, audit) ne bloquent plus la validation.
* [Yavsc.Org] Correction du flux MVC de creation de commentaire: `SaveChangesAsync(userId)` est utilise pour renseigner les champs d'audit requis (`UserCreated`/`UserModified`).
* [Yavsc.Api.Test] Stabilisation des fixtures de seed billing: remplissage des metadonnees d'audit (`UserCreated`, `UserModified`, dates) pour eviter les echecs SQLite `NOT NULL`.

## [1.0.8-rc13] - unstable

### Added

* [PostIt] Integration d'un selecteur de lieu RDV base sur Mapsui (carte interactive dans le formulaire `Rdv`).
* [PostIt] Ajout d'un marqueur de position et d'une action de recentrage sur la carte RDV.
* [PostIt] Ajout d'un service de reverse geocoding pour suggerer une adresse a partir des coordonnees carte.
* [PostIt] Cache et debounce des resolutions d'adresse RDV pour limiter les appels reseau et lisser l'UX.
* [PostIt.Tests] Nouvelles non-regressions sur le panneau d'adresse suggeree RDV et le comportement de la carte.
* [Yavsc.Abstract] Activation de `#nullable enable annotations` sur les fichiers legacy avec annotations nullable.
* [Yavsc.Server] Activation de `#nullable enable annotations` sur les fichiers legacy avec annotations nullable.

### Changed

* [PostIt] Generalisation de la barre de statut d'action (severite explicite) sur pages principales, dialogues et formulaires billing.
* [PostIt] Harmonisation des messages de statut utilisateur en francais.
* [PostIt] Renforcement des gardes de navigation dans les flux de gestion des membres de cercle.
* [PostIt] Le flux RDV conserve l'adresse saisie manuellement et propose l'adresse resolue comme suggestion explicite.
* [PostIt] Le flux de geolocalisation RDV tolere les positions proches dans le cache de suggestion d'adresse.

### Fixed

* [PostIt.Desktop] Correction d'un crash au demarrage OIDC (`No authority specified`) via durcissement des valeurs par defaut de configuration d'authentification.
* [PostIt] Correction de la persistance des settings: l'etat runtime de statut n'est plus serialize dans le JSON utilisateur.
* [PostIt.Tests] Ajout d'un verrou de non-regression sur le premier chargement des settings.
* [PostIt] Correction du binding de la date RDV: `DatePicker.SelectedDate` est aligne sur un proxy `DateTimeOffset?` (`EventDateSelection`).

## [1.0.8-rc12] - unstable

### Added

* [PostIt] Nouveau helper d'image `ImageHelper` pour charger des bitmaps depuis les ressources et depuis le web.
* [PostIt] Affichage de l'avatar XS dans la liste des performers d'activites, avec fallback visuel (initiale utilisateur).
* [PostIt.Tests] Nouveaux tests autour des URLs avatar et de la source d'autorite.
* [contrib] Ajout d'un `README.md` utilitaire pour les symboles/icones.

### Changed

* [PostIt] Les avatars ne sont plus relies en string sur `Image.Source`: ils sont telecharges et lies en `Bitmap`.
* [Yavsc.Api.Client] `ActivityApiClient` accepte une base d'avatar dediee et construit les URLs avatar depuis l'autorite d'identification.
* [PostIt] Les clients Activites/Billing utilisent maintenant `ApiUrl` en lecture dynamique: un changement via Parametres prend effet sans redemarrer l'application (apres sauvegarde et rafraichissement de la page).
* [PostIt] Le header de `MainPage` n'utilise plus `ScrollViewer`; remplacement par une barre de commandes basee sur `WrapPanel`.
* [PostIt] Alignement de la navigation blogs: renommage `PushMainPageAsync` -> `PushBlogsPageAsync` et ajustement de `HomePageViewModel`.

### Fixed

* [PostIt.Android] Correction d'un 404 sur la page Activites au premier lancement: la configuration embarquee pointait `ApiUrl` vers le host Blogs au lieu de l'API metier.
* [PostIt] Correction du bouton Sauver de la page Parametres: binding vers `SaveCommand` pour persister correctement `ApiUrl`/`BlogsApiUrl`.

## [1.0.8-rc11] - unstable

### Added

nothing

### Changed

* [Yavsc.Api.Test] Mise a jour de `Microsoft.EntityFrameworkCore.Sqlite` vers `10.0.11` afin de supprimer l'alerte NU1903 liee a `SQLitePCLRaw.lib.e_sqlite3` 2.1.11.
* [Yavsc.Org] Nettoyage de la configuration NuGet pour le restore: suppression du fichier local `Directory.Packages.props` au profit du fichier racine centralise.
* [Yavsc.Org] Suppression de references de packages redondantes dans le projet, sans impact fonctionnel attendu.

### Fixed

* [Yavsc.Api.Test] Le restore n'emet plus le warning de vulnerabilite `NU1903` sur `SQLitePCLRaw.lib.e_sqlite3`.
* [Yavsc.Org] Suppression d'une vulnerabilite de severite elevee sur AutoMapper apres publication et consommation de la nouvelle version candidate de `HigginsSoft.IdentityServer8`.

## [1.0.8-rc10] - unstable

### Added

* [PostIt] Une page d'historique des commandes billing permet maintenant d'ouvrir une commande existante.
* [PostIt] Une vue "Demandes en cours" en lecture seule est disponible pour le performer, filtrée sur les statuts actifs (Inserted, Accepted, InProgress).
* [Yavsc.Org] Nouvelles entités `Country` et `PerformerCodeInputValidation` pour piloter la validation du code entreprise performer par pays.

### Changed

* [PostIt] La page détail billing se préremplit depuis une commande existante (Rdv, Brush, MBrush) et passe en mode mise à jour.
* [Yavsc.Org] Le formulaire `Manage/SetActivity` inclut désormais le pays d'exercice (`fr`, `en`, `pt`) et applique la regex associée au champ `SIREN`.
* [Yavsc.Org] La vérification externe du numéro d'entreprise est conservée uniquement pour le pays `fr`.

### Fixed

* [PostIt] Le flux historique n'est plus limité à une simple liste: l'action d'ouverture charge la commande cible puis navigue vers la page détail.
* [Yavsc.Org] Le champ `SIREN` n'est plus validé avec une règle unique indépendante du pays d'exercice.

## [1.0.8-rc9] - unstable

### Added

nothing

### Changed

masquage non-owner côté backend de l'ACL du billet

### Fixed

On a maintenant le comportement attendu bout en bout:

ACL chargée depuis le BlogPostDto
noms de cercles affichés dans le dialogue ACL côté PostIt

## [1.0.8-rc8] - unstable

### Added

nothing

### Changed

nothing

### Fixed

The PostIt publish toggle button

## [1.0.8-rc7] - unstable

### Added

* [PostIt] The search pattern now persists

### Changed

* The blog spot path is now `/api/v1/blogspot` (yet in last release)

### Fixed

* [Yavsc.Org] (Ticket #45) La forme de l'email de l'utilisateur est maintenant validée avant l'envoi du formulaire d'enregistrement

## [1.0.8-rc6] - unstable

### Added

* a code cleanup,
* a first Xamarin.UITest is successful, but disabled, because breaking the actual CI process,
* Android app starts, the login process succeeds

### Changed

L'identifiant de l'application client Android a changé, il passe en minuscules :
`fr.pschneider.postit`

### Fixed

a bug posting and retrieving ACL from the backend,
the ACL now comes along with the article,
[TODO][PostIt] keep ACL along with the article

## [1.0.8-rc1] - unstable

### Added
- `BlogAclApiTests.PostCircleAuthorization_returns_201_when_payload_mirrors_PostIt_shape_against_existing_circle_named_test`
  : test de non-régression qui épingle la forme exacte du payload
  que PostIt envoie à `POST /api/v1/blogacl` (un objet
  `PostAccessControlRulePayload` avec `CircleId` et `BlogPostId`).
  C'est le verrou côté test du fix applicatif PostIt + serveur.
- `BlogAclApiTests.PostCircleAuthorization_never_returns_500` : une
  `[Theory]` couvrant quatre shapes de payload (`{ circleId }`,
  corps vide, `{ blogPostId }` seul, `{ circleId, blogPostId: 0 }`)
  qui doivent tous retourner un statut différent de 500. Toute
  réintroduction d'un chemin 500 dans le futur fera rougir ce test.
- `BlogAclApiTests.PostCircleAuthorization_dosent_return_500` et
  `..._dosent_return_500_on_success` : entry points `[Fact]` qui
  appellent la `[Theory]` ci-dessus avec un payload spécifique
  chacun, pour pouvoir filtrer en isolation depuis la ligne de
  commande ou le CI.
- Règle « Pas de `object` dans le code source applicatif » ajoutée
  à `CONTRIBUTING.md` : types de retour, paramètres, champs,
  propriétés, variables locales doivent être typés statiquement.
  `dynamic` est interdit pour les mêmes raisons.

### Changed
- `BlogAclApiController.CheckOwner` devient `CheckOwnerAsync` et
  utilise `FirstOrDefaultAsync` au lieu de `First`, supprimant
  l'appel LINQ synchrone sur le fil de la requête et retournant
  `false` sur cercle manquant (le contrôleur mappe déjà cela vers
  `ChallengeResult`).
- `BlogsWebServerFixture` seed `alice`, son `Circle` et son
  `BlogPost` une seule fois au démarrage du host, sur la
  `SqliteConnection` partagée (`Cache=Shared`). Le précédent
  `EnsureDeleted` au début de chaque test fermait la connexion
  statique et détruisait le store `:memory:` pour tous les autres
  `DbContext` ; il est retiré au profit d'un `EnsureCreated`
  idempotent.

### Fixed
- `POST /api/v1/blogacl` ne retourne plus 500 sur les payloads
  dont `BlogPostId` est absent ou à zéro. Le contrôleur rejette
  `BlogPostId <= 0` avec `400 BadRequest` avant que la requête
  n'atteigne `SaveChangesAsync`. L'incident de prod du 2026-08-21
  sur mercure (PostIt envoyant seulement `circleId`, le serveur
  voyant `BlogPostId = default(long) = 0` et EF Core levant
  `InvalidOperationException` sur l'INSERT) n'est plus atteignable.
- PostIt `PostAclDialogViewModel.AddAsync` envoie désormais le
  payload explicite `PostAccessControlRulePayload { CircleId,
  BlogPostId }` au lieu de l'ancien `CircleAuthorization {
  CircleId }`. Le DTO serveur `PostAccessControlRulePayload` est
  introduit dans `Yavsc.Abstract` pour porter le contrat.

## [1.0.7] - preview

### Added
- Per-post ACL in PostIt: a new “Manage ACL” page, opened from the ACL
  button on a selected post, lets the post author grant or revoke
  grants for individuals or circles. The server scopes each grant
  operation to `caller == post.AuthorId` and returns `404` (not `403`)
  for posts the caller does not own, so the existence of another
  user's post is not leaked.
- Circle membership API + UI: three new REST endpoints under
  `/api/circle/{id}/members` (`GET` list, `POST` add, `DELETE`
  remove) and a new “Members” column on the *My Circles* page with an
  “Add a member” button that opens a search modal. The search modal
  reuses `IUserDirectory` (introduced by the `IContactService` split
  in this same release) — exactly the use case the abstraction was
  carved out for.
- Publish toggle for blog posts: a new `PUT /api/BlogApi/{id}/publish`
  endpoint, and a `Published` checkbox in the post toolbar that
  toggles a `BlogSpotPublication` row for the post. The publish
  signal flows through the pre-existing `PermissionHandler.IsPublic`
  path, so no new column was needed and the server-side authorisation
  logic is unchanged.
- `UserSearchApiController` in `Yavsc.Blogs`:
  `GET /api/user-search?q=...&e=...&take=...`. Any-authenticated-
  caller endpoint that exposes the user's email under a closed-
  community assumption (documented in the controller's XML doc).
  Wired to the PostIt Desktop address book so the user search modal
  picks it up.
- `IYavscApiClient` abstraction in `Yavsc.Api.Client`. The transport
  for the blog/circle/blog-acl/user-search clients is now accessed
  through this interface, so `PostIt.Tests` can stub the HTTP layer
  without spinning up a real WebAPI host.
- Forgejo Actions release workflow: a `.forgejo/workflows/release.yml`
  pipeline that builds and publishes a release with the PostIt APK
  on tag push. Written in pure bash (the runner image has no Node),
  uses `jq` for JSON body construction and response parsing, uses the
  runner-provided `GITHUB_TOKEN` (no repo-level secret needed),
  validates the CHANGELOG section heading before allowing the tag
  to ship.
- `make release V=<version>` target: creates a `release/<V>` branch
  from `main`, bumps the `<Version>` property in every `.csproj` via
  `dotnet-gitversion /updateprojectfiles`, commits the bump on the
  release branch, and pushes to `origin`. Fails fast if the working
  tree is dirty or if `HEAD` is not on `main`.
- Forgejo status badges in the README.

### Changed
- The new Publish toggle replaces the “Visibility enum” approach
  originally drafted in this branch: the existing `BlogSpotPublication`
  table already carried enough information to expose a publish
  switch, so no schema change was needed. The original `feat(blog):
  add Visibility { Private, Public }` commit and its EF migration
  were reverted in favour of the endpoint-only toggle.
- `BlogPost` DTO and `IBlogPost` moved from `PostIt.Models` to
  `Yavsc.Abstract.Blogspot`, the shared assembly where the server-side
  entity and the wire DTO both live. Renamed `Yavsc.Blogspot.BlogPost`
  to `BlogPostDto` to make the wire/entity distinction explicit.
- `BlogAclApiController` and `CircleApiController` moved from
  `Yavsc.Api` (not yet enabled in production) to `Yavsc.Blogs`, where
  they belong next to the `BlogSpotService` they depend on.
- `IContactService` split from `IUserDirectory`: the two interfaces
  previously conflated the local address-book access (mobile-only,
  via `Contacts.Default`) and the Yavsc user-search access
  (Desktop-only, via `/api/user-search`) behind a single facade. The
  split restores the `ContactDto.Emails` multi-value shape that was
  being silently flattened to a single string before.
- CI: the Forgejo Actions build now compiles `.csproj` projects
  directly inside the runner container (which ships the .NET SDK +
  Android workload), instead of relying on a separate Docker build
  step. Node-based third-party actions were replaced with bash + curl
  + `jq`. The validate-release job parses the CHANGELOG section
  heading to derive the channel (`stable` / `preview` / `unstable`)
  rather than the patch-version parity alone.

### Fixed
- `CircleApiController` used to read the caller's user id via
  `FindFirstValue(ClaimTypes.NameIdentifier)`, which does not match
  when JWT Bearer middleware has `MapInboundClaims = false`. Switched
  to `User.GetUserId()` (tries `sub` first, then
  `ClaimTypes.NameIdentifier`, then `nameid`). This was a latent
  bug visible in tests but easy to ship to production if a host
  ever disabled the remap.
- `CircleApiController` and `BlogAclApiController` reads and writes
  were not always scoped to the caller's own data. Tightened the
  authorisation checks: cross-user reads now return `404`, not the
  raw record.
- `validate-release` CHANGELOG channel check used to parse the
  patch-version parity only, which disagreed with the channel
  suffix in the section heading (e.g. `## [1.0.7] - preview`
  would be flagged as `stable` from the parity alone). The job now
  inspects the heading line and trusts the suffix when present.
- `.forgejo/workflows/release.yml`: the asset-upload URL now carries
  the asset name as a query-string parameter instead of a `curl`
  positional argument. The previous shape triggered Forgejo's
  “Missing `name` parameter” 400 in some cases.

### Removed
- The `## [Unreleased]` block has been moved into this section.
- The abandoned `Visibility { Private, Public }` enum and its EF
  migration, reverted in this release. The publish toggle covers
  the same user-visible switch without a schema change.

[Unreleased]: https://forgejo.pschneider.fr/notazof/yavsc/compare/HEAD
[1.0.8-rc16]: https://forgejo.pschneider.fr/notazof/yavsc/compare/1.0.8-rc15...1.0.8-rc16
[1.0.8-rc15]: https://forgejo.pschneider.fr/notazof/yavsc/compare/1.0.8-rc14...1.0.8-rc15
[1.0.8-rc1]: https://forgejo.pschneider.fr/notazof/yavsc/compare/1.0.7...1.0.8-rc1
[1.0.7]: https://forgejo.pschneider.fr/notazof/yavsc/compare/1.0.6...1.0.7
[1.0.6]: https://forgejo.pschneider.fr/notazof/yavsc/compare/1.0.5...1.0.6

## [1.0.6] - stable

### Added
- Self-hosted Forgejo Actions runner now drives the CI build for the
  yavsc repository, using the
  `pazof/yavsc-build-env:debian12-dotnet10-android36-v2` image pulled
  from Docker Hub. Workflow runs end-to-end: clone, restore, build,
  test, with NuGet.config picking up the `isn.pschneider.fr` feed.
- The build-env image now ships `jq` (Debian package, ≥ 1.7), so the
  release workflow can build JSON bodies and parse API responses
  without a hand-rolled `sed`-based extractor that was matching the
  wrong `id` field on minified responses.

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
- `.forgejo/workflows/release.yml`: PATCH on `/releases/{id}` no longer
  404s on existing releases. The previous `sed`-based `json_field`
  matched the last `id` on the line (the author's), so it tried to
  PATCH `/releases/1` (the first user of the instance) instead of the
  actual release id. Switched to `jq` for both body construction and
  field extraction.

[1.0.6]: https://forgejo.pschneider.fr/notazof/yavsc/compare/1.0.5...1.0.6
