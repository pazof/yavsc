# Architecture de Yavsc

## Vision générale

Yavsc est une plateforme de mise en relation client/fournisseur,
spécialisable par domaine d'activité, avec une origine musicale.

Elle gère des **devis** (pas de facturation directe), et s'intègre
dans des workflows collaboratifs pouvant aboutir à des livrables
sous licence libre.

## Pages de détail

Chaque sujet est détaillé dans sa propre page (sous
`doc/architecture/`) ; cette racine tient lieu de sommaire.

| Sujet                                          | Page                                                              |
|------------------------------------------------|-------------------------------------------------------------------|
| Workflow multi-parties (client / fournisseur / coordinateur, sous-traitance, états) | [workflow-multi-parties.md](architecture/workflow-multi-parties.md) |
| Domaine musical (titres collaboratifs)         | [domaine-musical.md](architecture/domaine-musical.md)             |
| Licences (`LicenceModele`, seed CC/ODbL, badge)| [licences.md](architecture/licences.md)                           |
| Domaines d'activité (arbre, `DomaineActivite`) | [domaines-activite.md](architecture/domaines-activite.md)         |
| Dictionnaires métier (héritage en arbre)       | [dictionnaires-metier.md](architecture/dictionnaires-metier.md)   |
| Offre fournisseur + frontmatter (`ClasseFormulaire`, `ClasseDevis`, parsing) | [offres-frontmatter.md](architecture/offres-frontmatter.md) |
| PostIt / OIDC (client desktop, custom URI scheme, silent refresh) | [postit-oidc.md](architecture/postit-oidc.md)        |
| Notifications Web/PostIt, ChatHub et service Android SignalR | [notifications.md](architecture/notifications.md) |

## Stack technique

- **Backend** : ASP.NET Core, C#
- **ORM** : Entity Framework Core (migrations générées)
- **Base de données** : PostgreSQL (provider Npgsql)
- **Frontend** : Razor views
- **Parsing frontmatter** : YamlDotNet
- **Client desktop** : Avalonia 12 (PostIt — détails dans
  [postit-oidc.md](architecture/postit-oidc.md))

## Droits de Yavsc

### Administration

Le groupe des administrateurs prend la charge de :

- la Gestion des licences
- la Gestion des groupes d'utilisateurs (les modérateurs, en particulier)
- la Gestion des projets
- la Gestion des utilisateurs

### Gestion des utilisateurs et des groupes

En supposant que les certificats de letsencrypt sont au groupe
`www-data`, on peut créer l'utilisateur `yavsc` avec les droits
suivants :

```bash
sudo addgroup yavsc --system
sudo adduser --ingroup yavsc --add-extra-groups www-data \
 --disabled-password --system yavsc
```

## Catalogue Web de podcasts

`/Podcast` (vue `src/Yavsc.Org/Views/Podcast/Index.cshtml`) exige une
session Identity Yavsc.Org, y compris pour chaque requête de lecture.
Yavsc.Org interroge Yavsc.Api côté serveur ; aucun jeton n'est transmis
au navigateur et aucun second catalogue de stockage n'est créé.
Les replays MP4 prêts prennent en charge GET, HEAD et les plages
d'octets. Les directs et replays non encore convertis utilisent HLS
natif ou hls.js (servi localement, installé par `npm install` et copié par
`npm run build:podcasts`, également inclus dans la copie d'assets de
`npm run build:js`). Sans support vidéo compatible, la page affiche un
avertissement et invite à utiliser PostIt. Cette page ne crée ni ne
supprime de podcast.
Depuis le lecteur PostIt, « Ouvrir dans le navigateur » ouvre `/Podcast`
sur l'hôte `Authentication.Authority`, pas la playlist API authentifiée.
La session Web reste distincte de celle de PostIt : le navigateur peut
donc demander une connexion.

Il n'y a pas de section de configuration `PodcastWeb`, ni de client OAuth
technique à enregistrer. L'adresse API est celle de `Site:ApiUrl`, en HTTPS
(avec ou sans suffixe `/api/v1`). Yavsc.Org est l'autorité : comme pour
l'envoi d'avatar, `IdentityServerTools.IssueJwtAsync` utilise sa signature
existante pour émettre un jeton de cinq minutes au nom de l'utilisateur
connecté (`sub`, audience `api`, scope `api` attendu par Yavsc.Api).
Ce jeton reste côté serveur. Une adresse API manquante renvoie 503 ;
une panne de l'API renvoie une erreur explicite, pas un catalogue vide.

### Emissions preparees

PostIt memorise dans ses parametres utilisateur les cameras et microphones
detectes, leurs identifiants selectionnes et le titre par defaut. La page
les restaure sans redetection automatique ; le bouton de detection actualise
le materiel disponible en conservant les choix encore presents. Le choix
« sans microphone » est memorise lui aussi. Charger le titre d'une emission
preparee ne remplace pas le titre par defaut ; une saisie explicite le modifie.
Les erreurs de sauvegarde sont affichees dans la page.
Les appels des salons de discussion reutilisent ces memes choix camera/micro
et enregistrent leurs changements dans les memes parametres. La page Podcasts
les recharge a son activation. Les appels exigent toujours un microphone :
le choix podcast « sans microphone » laisse donc le micro d'appel non selectionne.
L'adresse LAN/VPN choisie pour les appels est aussi memorisee, mais restauree
uniquement si elle est encore presente sur une interface locale active.
Sinon, une nouvelle selection est requise. Le consentement au partage
d'adresse avec le pair n'est jamais memorise.

PostIt permet de preparer, modifier et supprimer une emission avant son
demarrage : titre, date/heure locale, duree entiere positive en minutes et
annonce publique optionnelle. Les horaires sont previsionnels : la camera,
le demarrage et l'arret restent manuels. Les heures locales inexistantes ou
ambigues au changement d'heure sont refusees.

La table existante `LiveFlow` conserve la preparation et son proprietaire ;
`PodcastId` (GUID unique) relie cette emission au manifeste media sur disque.
`PodcastResponse` reste un DTO, enrichi de `FlowId`, `ScheduledStartAt`,
`PlannedDurationMinutes` et `PublicAnnouncement`. Un etat `Planned`
precede le direct ; la date previsionnelle n'est jamais confondue avec
`CreatedAt` du direct ni la duree previsionnelle avec les secondes enregistrees.
Les anciens manifestes sans `LiveFlow` restent lisibles ; aucune copie ou
migration de leurs medias n'est requise. Le demarrage immediat historique
reste disponible sans preparation.

L'API propose `POST /api/v1/podcasts/planned`,
`PUT /api/v1/podcasts/{id}/planned` et `POST /api/v1/podcasts/{id}/start`.
La preparation n'est visible que du proprietaire, meme avec une annonce ;
les nouvelles routes de mutation exigent le scope `api`. Le demarrage
conserve le GUID et les tentatives concurrentes reutilisent le meme direct.
Une emission demarree ne peut plus etre reprogrammee ou redemarree apres
sa fin. Les medias restent geres par `ILiveProcessor`, avec ses quotas,
expiration, reprise et preparation MP4. Une suppression conserve un
marqueur en base pour ne pas ressusciter une emission deja supprimee.
Le verrou de coordination suit le deploiement mono-processus du stockage
media actuel ; un deploiement multi-instance necessiterait un stockage et
une coordination distribues.

Seul `GET /api/v1/podcasts/announcements` est anonyme : titre, nom d'auteur,
horaire UTC, duree et texte public, sans URL de flux ni metadonnees privees.
`/Podcast/Announcements` affiche ces annonces avec encodage Razor et sans
jeton utilisateur. Retirer le texte public avant le debut retire l'annonce.
Les annonces restent disponibles apres le direct jusqu'a suppression du
podcast. Les pages et toutes les routes de lecture restent authentifiees.
Appliquer la migration `PodcastScheduling` au deploiement, apres celle des
dons ; aucune migration de production n'est appliquee automatiquement ici.

### Dons PayPal aux performers

Les dons sont gérés par Yavsc.Org, pas par un lien de paiement externe
configuré par l'opérateur :

1. Depuis **Mon compte > Dons PayPal**, le performer renseigne l'adresse
   de son propre compte PayPal (`/Donation/Settings`). Un champ vide
   désactive les nouveaux dons. Seuls les performers actifs ayant
   configuré leur compte proposent un bouton de don dans le catalogue.
   Le formulaire est aussi accessible depuis le catalogue de podcasts et
   le profil professionnel Web. Dans PostIt : **Mon profil > Gérer mon
   activité professionnelle > Dons PayPal**, puis **Enregistrer**.
   L'API privée `GET/PUT /api/v1/account/performer-profile` transporte
   `DonationPayPalEmail` uniquement pour le compte connecté ; une chaîne
   vide désactive les dons, une propriété omise ou nulle préserve
   l'adresse existante pour les anciens clients.
2. Le donateur connecté choisit un montant en euros (`/Donation/Create`).
   Yavsc conserve le don et son bénéficiaire avant de créer le checkout.
3. Yavsc utilise les paramètres marchands existants
   `Authentication:PayPal` (mode sandbox/production et identifiants NVP).
   Express Checkout désigne le compte du performer par
   `PAYMENTREQUEST_0_SELLERPAYPALACCOUNTID` : le paiement va directement
   au performer ; Yavsc ne collecte pas les fonds et ne les reverse pas.
4. PayPal recueille l'accord du donateur puis le renvoie dans Yavsc.
   Une confirmation POST avec protection antiforgery déclenche le
   paiement, après vérification côté serveur du token, du montant,
   de la devise, de la référence et du bénéficiaire. Un simple retour
   du navigateur n'est jamais considéré comme une preuve de paiement.
5. Seule une réponse PayPal vérifiée permet d'afficher le don comme payé.
   Les notifications IPN reçues sur `/Donation/Notify` sont revalidées
   auprès de PayPal, puis rapprochées du bénéficiaire, du montant, de la
   devise et de la transaction. Elles permettent aussi de confirmer
   ultérieurement un paiement Pending ou une réponse de capture perdue.

`Site:ExternalUrl` doit être l'URL HTTPS publique de Yavsc.Org, utilisée
pour les retours et notifications (jamais le Host fourni par le client).
PayPal doit pouvoir joindre `/Donation/Notify`. Le compte marchand
doit être autorisé par PayPal pour les paiements directs à un tiers ;
valider ce parcours en sandbox avec deux comptes distincts avant mise
en production. Les tests locaux utilisent un serveur PayPal simulé,
pas de véritables transactions.

`PerformerDonations` conserve la référence, le donateur, le performer,
le bénéficiaire figé à la création, le montant et l'état du paiement.
La migration `PerformerPayPalDonations` ajoute ce suivi et l'adresse
PayPal optionnelle du performer. Les profils ayant un historique de
dons ne peuvent pas être supprimés. Les rééditions du profil général
ne remplacent pas la configuration du bénéficiaire.

Le paiement n'est capturé qu'une fois : contrôle de concurrence,
référence de déduplication PayPal (`MSGSUBID`) et transaction unique.
En cas de réponse ambiguë, le don reste en vérification sans nouvelle
capture automatique. Le donateur retrouve son suivi dans **Mon compte >
Mes dons** (`/Donation`). Si l'IPN ne réconcilie pas cet état, l'opérateur
doit vérifier la transaction auprès de PayPal avant toute nouvelle
tentative. Les dons sont facultatifs et n'ouvrent pas droit à un reçu
fiscal émis par cette page.

## À documenter ensuite

- Modèle `ProjetMusical`
- Gestion des fichiers (audio, partition, MIDI)
- Workflow de validation de licence par l'administration
- Workflow de modération des termes métier
- Workflow complet de parsing et validation du frontmatter à la sauvegarde
