# Appels audio-vidéo PostIt — architecture cible

## Statut et périmètre

Ce document décrit l'architecture cible et l'implémentation commencée :
les appels ne sont pas encore disponibles. La préférence de confidentialité
est persistée et éditable dans le profil. Les premiers contrats de
signalisation sont intégrés au chat, désactivés par défaut ; aucun transport
média natif n'est encore raccordé.
Il complète la [topologie de PostIt](postit.md) et les
[mécanismes de notification existants](notifications.md).
œ
La cible couvre :

- les appels audio-vidéo bidirectionnels entre deux utilisateurs ;
- un réglage utilisateur booléen persistant permettant de masquer son adresse
  IP aux autres participants en imposant TURN ;
- les appels de groupe ;
- les clients Browser, Android et Desktop.

## Choix des protocoles

| Besoin | Choix | Rôle |
|---|---|---|
| Médias temps réel | WebRTC | Audio-vidéo bidirectionnel à faible latence, adaptation au réseau |
| Chiffrement du transport média | DTLS-SRTP | Établissement des clés et protection des médias |
| Signalisation | SignalR sur HTTPS | Invitation, acceptation/refus, négociation SDP, candidats ICE, fin d'appel |
| Connectivité | ICE, STUN et TURN | Sélection du chemin réseau, découverte d'adresse et relais |
| Audio | Opus | Codec privilégié pour la voix |
| Vidéo | VP8 ou H.264 | Codec négocié selon les capacités réelles des clients |
| Appels de groupe | SFU compatible WebRTC | Réception et redistribution sélective des flux |

SignalR transporte les messages de contrôle, **jamais les flux audio-vidéo**.
La signalisation doit réutiliser le [ChatHub existant](../../src/Yavsc.Server/Hubs/ChatHub.cs),
déjà exposé par [Yavsc.Api](../../src/Yavsc.Api/Program.cs), et ses salles de
discussion. Il n'est pas nécessaire de créer un second service de salles.
Les contrats d'appel et leurs autorisations restent à définir explicitement :
l'appartenance à une salle ne vaut pas acceptation d'un appel.
Le choix des bibliothèques natives, du SFU et des codecs effectivement
disponibles reste à valider par un prototype multi-plateforme.

## Topologies média

### Appels à deux

```text
PostIt A ---- SignalR / HTTPS ---- Serveur ---- SignalR / HTTPS ---- PostIt B
             Invitation, acceptation, SDP, candidats ICE, fin

PostIt A ================= WebRTC direct ========================= PostIt B
                               ou
PostIt A ================= relais TURN =========================== PostIt B
```

Privilégier un chemin direct lorsque les préférences de confidentialité le
permettent. Prévoir TURN dès la première version : STUN seul ne suffit pas sur
certains réseaux mobiles, NAT ou réseaux d'entreprise.

Déployer par exemple coturn, avec des transports UDP et des solutions de
repli TCP/TLS, dont TURN/TLS sur le port 443. Ce dernier améliore la
connectivité sans garantir la traversée de tous les pare-feu et proxys.

### Appels de groupe

```text
PostIt A ---- WebRTC, éventuellement via TURN ----+
PostIt B ---- WebRTC, éventuellement via TURN ----+---- SFU
PostIt C ---- WebRTC, éventuellement via TURN ----+
```

Utiliser un **SFU**, plutôt qu'un maillage de connexions entre participants :
chaque client envoie ses pistes au serveur, qui redistribue les flux utiles.
Cela évite de multiplier les envois montants avec le nombre de participants.
Prévoir la sélection des pistes reçues et, selon les capacités des clients,
le simulcast ou le codage vidéo scalable pour adapter débit et résolution.

Le SFU ne remplace pas TURN : un client peut avoir besoin d'un relais pour
atteindre le SFU, ou l'imposer par sa préférence de confidentialité.

Les contrats de signalisation doivent représenter dès le départ un appel et
ses participants, sans être limités à un couple appelant/appelé. Pour ajouter
un troisième participant à un appel direct, établir les connexions au SFU,
puis fermer les connexions directes après confirmation du nouveau transport.
En cas d'échec, conserver l'appel à deux et signaler l'échec de l'ajout.
Une topologie SFU pour tous les appels reste une alternative à évaluer si la
simplicité d'exploitation prime sur le coût serveur.

## Préférence persistante de masquage d'adresse IP

### Contrat fonctionnel

Le booléen de compte `HideIpAddressInCalls` est persisté côté serveur et
accessible en lecture/écriture uniquement par
l'utilisateur concerné via `GET /api/v1/account/me` et
`PUT /api/v1/account/me`, ainsi que dans « Mon profil » de PostIt.
Le réglage est partagé entre ses appareils ; une préférence locale seule
ne suffit pas.

Valeur initiale : `false`, pour les nouveaux comptes et les comptes existants
après migration. Le champ de mise à jour est nullable : son absence ou `null`
conserve la valeur existante, notamment pour les anciens clients ; `false`
explicite désactive la préférence.
`false` autorise le chemin direct mais n'interdit pas TURN lorsqu'il est
nécessaire à la connectivité. `true` impose un transport relayé.

Présenter le réglage comme « Masquer mon adresse IP aux autres participants »,
en précisant le surcoût potentiel en latence et en bande passante serveur.
Il ne procure pas l'anonymat : les services contactés, notamment l'API, le
serveur de signalisation et TURN, peuvent toujours connaître l'adresse IP.

La migration EF Core `HideIpAddressInCalls`, dans le projet Yavsc.Org, ajoute
la colonne booléenne non nullable à `AspNetUsers`. Elle doit être déployée
avant de servir les nouvelles lectures et écritures de profil. Cette étape
n'installe ni TURN ni SFU et n'implémente pas encore les règles de négociation
décrites ci-dessous.

### Application à la négociation

- Charger la préférence avant de créer le transport média. Si sa lecture
  échoue, signaler l'erreur et bloquer l'établissement plutôt que supposer
  que le masquage est désactivé.
- Lorsque le réglage est activé, configurer une politique ICE **relay-only**
  (`iceTransportPolicy: "relay"` dans le navigateur, équivalent natif ailleurs)
  avant la collecte des candidats.
- Ne publier aucun candidat local `host` ou `srflx`, ni aucune adresse locale
  dans les descriptions SDP ou messages de signalisation. Utiliser les API du
  moteur WebRTC et vérifier les descriptions produites, plutôt qu'une
  réécriture ad hoc du SDP.
- Appliquer cette contrainte aux offres, réponses, renégociations,
  redémarrages ICE et transitions vers le SFU.
- Le masquage d'un participant impose le relais de **son propre transport**,
  même si les autres participants n'ont pas activé cette préférence.
- Si TURN est indisponible ou qu'aucune paire de candidats relayée n'aboutit,
  afficher une erreur explicite ; **aucun repli direct automatique**.

Le serveur détermine la politique attendue depuis la préférence persistée,
contrôle les messages de signalisation et distribue les paramètres autorisés.
Le client applique la politique ICE ; les tests doivent vérifier le chemin
réellement sélectionné, pas seulement la configuration demandée.

Une modification pendant un appel doit être prise en charge explicitement :
l'activation suspend l'émission et remplace le transport avant de reprendre
les médias en mode relayé. L'interface ne doit pas annoncer le masquage tant
que ce remplacement n'est pas confirmé. Elle précise qu'une adresse déjà
divulguée auparavant ne peut pas être retirée aux participants.
La désactivation peut attendre l'appel suivant, sans abandonner le relais
en cours.

## Intégration dans PostIt

Séparer le cycle de vie partagé de l'appel du moteur média :

- code partagé : invitation, sonnerie, acceptation/refus, participants,
  préférences, état de connexion, coupure micro/caméra et raccrochage ;
- Browser : API WebRTC du navigateur via interop JavaScript ;
- Android et Desktop : moteur WebRTC natif et intégration de la capture,
  des périphériques et du rendu dans Avalonia.

Le prototype doit vérifier les bindings maintenus, leurs licences, les
architectures processeur supportées, les codecs et l'intégration Avalonia.
Ne pas supposer qu'une bibliothèque .NET couvre à elle seule la capture,
l'annulation d'écho et le rendu sur toutes les plateformes.

Prévoir les permissions caméra/micro, les changements de périphérique,
les interruptions Android, la reconnexion et le redémarrage ICE.
Les appels entrants en arrière-plan nécessitent une intégration aux
notifications système et au cycle de vie Android ; une connexion SignalR
active ne peut pas être présumée.

## Autorisations, confidentialité et exploitation

- Authentifier la signalisation et vérifier côté serveur l'appartenance à
  l'appel pour chaque commande et événement, y compris rejoindre un groupe,
  inviter, publier une piste et recevoir les flux.
- Prévoir des invitations expirables et gérer les réponses concurrentes de
  plusieurs appareils sans accepter plusieurs fois le même participant.
- Délivrer des identifiants TURN temporaires et des autorisations SFU limitées
  à l'appel et au participant ; aucun secret permanent dans les clients.
- Limiter les invitations, les participants et les débits selon une politique
  produit à définir ; surveiller la capacité et le coût TURN/SFU.
- Ne pas journaliser systématiquement SDP, candidats ICE ou identifiants TURN.
  Préférer des métriques de qualité et des identifiants techniques d'appel.
- Distinguer chiffrement du transport et chiffrement de bout en bout :
  en connexion directe ou via TURN, les médias restent chiffrés entre les deux
  clients ; avec un SFU classique, DTLS-SRTP se termine au SFU. Un chiffrement
  de bout en bout des groupes demanderait un mécanisme supplémentaire et une
  gestion des clés à concevoir, sans être promis par ce plan.

## Étapes proposées

1. **Prototype multi-plateforme** : valider capture, rendu, codecs et WebRTC
   entre Browser, Android et Desktop, y compris via TURN.
2. **Appels à deux** : signalisation authentifiée, cycle de vie d'appel,
   coturn, préférence persistante et politique relay-only opérationnelle.
3. **Appels de groupe** : sélectionner et intégrer le SFU, autoriser les
   participants, gérer les pistes et la transition depuis un appel direct.
4. **Fiabilisation** : arrière-plan Android, réseau mobile, reprise après
   interruption, accessibilité des contrôles et observabilité.

Les appels de groupe font partie de la cible finale, pas d'une simple
extension facultative. Leur modèle de participants doit être anticipé dès
la signalisation des premiers appels à deux.

## Critères de validation

- Audio et vidéo bidirectionnels sur les combinaisons de plateformes ciblées.
- Appel direct autorisé quand le réglage est désactivé et relais disponible
  lorsque le réseau l'exige.
- Préférence conservée après reconnexion et relue sur un autre appareil.
- Avec masquage activé, absence d'adresses locales/publiques propres au client
  dans la signalisation transmise aux pairs et sélection effective d'un
  candidat local `relay`, confirmée par les statistiques WebRTC.
- Aucun repli direct si TURN échoue, y compris après redémarrage ICE,
  changement de réseau ou ajout d'un participant.
- En groupe, contrôle d'accès aux pistes, départ et arrivée de participants,
  adaptation de qualité et respect individuel de la politique TURN.
- Activation du masquage pendant un appel : suspension des médias,
  remplacement vérifié du transport et retour d'état fidèle dans l'interface.
- Échecs de permissions, de signalisation, de TURN et du SFU visibles,
  ressources caméra/micro et connexions libérées à la fin de l'appel.

## Première tranche intégrée : signalisation Android et Desktop Linux

Le périmètre initial retenu est **Android + Desktop Linux**. Il n'ajoute
aucun serveur ni aucune salle : les méthodes suivantes appartiennent au
[ChatHub](../../src/Yavsc.Server/Hubs/ChatHub.Calls.cs) existant.
Les [contrats partagés](../../src/Yavsc.Abstract/Chat/Calls/CallContracts.cs)
sont utilisés par le serveur et PostIt.

| Méthode | Effet |
|---|---|
| `InviteCall(roomName, callee)` | Invite un autre compte présent dans la salle et retourne un identifiant d'appel |
| `AcceptCall(callId)` | Accepte sur un seul appareil, notifie les deux pairs et ferme l'invitation sur les autres appareils |
| `SendCallSignal(callId, signal)` | Transmet l'offre, la réponse ou un candidat ICE uniquement à l'autre appareil accepté |
| `EndCall(callId)` | Annule, refuse ou termine l'appel depuis un appareil autorisé |

Événements : `CallInvitation`, `CallAccepted`, `CallSignal(callId, signal)`
et `CallEnded`. **Accepté ne veut pas dire connecté** : l'événement accepté
exprime le consentement et fournit la politique ICE et les paramètres
STUN/TURN, pas une réussite de l'échange média.

Le serveur vérifie les comptes, la présence dans la salle et le blocage
entre utilisateurs. Les appareils d'un compte reçoivent l'invitation ;
seul le premier qui accepte peut ensuite émettre la réponse et les
candidats. L'appelant reste lié à sa connexion d'origine. Les médias
ne sont jamais diffusés au groupe SignalR de la salle.

La préférence persistée est lue à l'invitation et relue à l'acceptation.
Pour cette première version à deux, si l'un des comptes demande le
masquage, **les deux transports doivent être relayés**, règle conservatrice
plus stricte que la politique individuelle cible. L'acceptation ne peut
pas diminuer la protection annoncée à l'invitation. Sans TURN configuré,
un appel imposant le relais échoue explicitement.

La signalisation refuse les candidats non relayés, les adresses associées
non masquées et les adresses directes dans les champs SDP contrôlés lorsque
le relais est imposé. Ces contrôles de forme ne constituent pas un audit
exhaustif de toutes les extensions SDP possibles : la non-divulgation doit
encore être prouvée avec les SDP et statistiques des moteurs réellement
intégrés. Aucun SDP n'est réécrit.

L'activation du masquage ou le changement de pseudo par l'API de profil
termine l'appel existant ; le client devra libérer les médias et proposer
un nouvel appel. La renégociation transparente du transport n'est pas
implémentée. La déconnexion d'un pair sélectionné, le départ ou l'exclusion
de la salle terminent également l'appel. Pas de reprise automatique après
reconnexion : elle crée une nouvelle identité de connexion.

Les appels sont temporaires, en mémoire et **sur une seule instance API**.
Une invitation expire après 45 secondes, une session acceptée après une
heure ; les identifiants TURN expirent cinq minutes plus tard. Cette borne
initiale ne remplace pas un futur renouvellement d'identifiants pour les
appels longs. Un compte ne peut avoir qu'un appel réservé ou accepté à la
fois. Le registre est limité à 128 appels et 256 candidats par appel.
La tranche ne permet qu'une offre initiale de l'appelant et une réponse ;
les redémarrages ICE et renégociations restent à implémenter.

Configuration serveur, section `AudioVideoCalls` :

- `Enabled` : `false` par défaut, à conserver tant que les moteurs ne sont
  pas validés ;
- `StunUrls` : tableau d'URI `stun:` ou `stuns:` ;
- `TurnUrls` : tableau d'URI `turn:` ou `turns:`, avec transport optionnel ;
- `TurnSharedSecret` : secret REST coturn, **uniquement côté serveur**,
  à fournir par le gestionnaire de secrets ou la variable d'environnement
  `AudioVideoCalls__TurnSharedSecret`, jamais dans le dépôt.

Les credentials TURN temporaires sont calculés côté serveur avec le
format REST coturn (expiration, nonce aléatoire et HMAC-SHA1). Aucun secret
de longue durée n'est envoyé aux clients. Aucun service TURN n'a été
déployé ou configuré par cette tranche.

Le [transport PostIt](../../src/PostIt/PostIt/Services/CallSignalingClient.cs)
utilise la connexion authentifiée du `ChatViewModel`, sans seconde connexion
ni interrogation HTTP. Il expose les commandes et événements typés et
signale les interruptions ; il ne capture aucun média. L'interface ne
propose pas encore de bouton d'appel ; une éventuelle invitation est
signalée comme non utilisable faute de moteur natif, sans acceptation
automatique.

### Travail natif restant avant activation

- Android : intégrer un adaptateur libwebrtc dans l'application existante,
  isoler le problème JNI observé, vérifier les versions Java/natives et
  conserver les diagnostics fatals utiles, sans journaliser SDP ou secrets.
- Desktop Linux : intégrer un moteur WebRTC natif dans PostIt.Desktop
  avec capture réelle et rendu Avalonia ; un processus Python de test
  GStreamer ne vaut pas cette intégration.
- Côté partagé : relier consentement, états réels du moteur, mise en
  sourdine, arrêt et libération des ressources à la signalisation.
- Valider un appel Android ↔ Desktop Linux avec caméra avant et microphone
  réels, rendu et audio distants, VP8/Opus, puis transport TURN sélectionné
  dans les statistiques, sans repli direct lorsque le masquage est demandé.

Les tests de cette tranche portent sur les contrats, autorisations, courses
d'acceptation, expirations, limites, interruption et préférence persistée.
La compilation des applications Android et Desktop ne prouve pas le
fonctionnement de WebRTC natif : le crash antérieur n'est pas résolu.

## Retour sur l'essai Android du 25 octobre 2026

Le laboratoire indépendant a été supprimé ; il ne constitue pas une base
de salles de discussion ni une implémentation de PostIt. La suite doit
réutiliser le ChatHub existant pour la signalisation.

L'essai en échec opposait un téléphone Android 15 arm64 à Chromium,
en mode TURN uniquement, avec signalisation HTTP locale et TURN/TCP
accessibles par redirections USB adb. Il n'utilisait ni ChatHub ni
PostIt.Desktop Linux.

Les observations doivent être distinguées :

- Camera2, la capture audio et un premier rendu EGL local ont démarré.
  Cela ne valide ni le transport ni le rendu distant.
- Le processus Android a subi un SIGABRT dans
  `libjingle_peerconnection_so.so`. Une première pile passait par
  `PeerConnection.nativeAddIceCandidate` ; après modification du traitement
  des candidats, un arrêt natif persistait sur `network_thread`.
- Le test a attendu un échange média qui n'est jamais arrivé. Le dernier
  rapport Browser indiquait `connection: new`, zéro octet audio/vidéo et
  aucune paire ICE sélectionnée ; aucun rapport Android n'a validé l'appel.
- Le problème a été reproduit avec les SDK libwebrtc 150.7871.04 et
  144.7559.15. L'attente des descriptions distantes, la résolution du
  `sdpMid` et l'ajout asynchrone des candidats n'ont pas suffi.

Il s'agit donc d'un **crash natif pendant la tentative d'établissement**,
pas d'un refus de codec ou d'un échec TURN identifié. Le dernier appel JNI
visible ne prouve pas la cause du crash. Sans message fatal détaillé ni
pile native symbolisée, un défaut du code expérimental, de son intégration
Java/JNI ou de libwebrtc reste à départager. Rien ne démontre une
impossibilité de WebRTC sur Android ou une défaillance du ChatHub.

Les essais Browser/GStreamer réussis utilisaient des sources synthétiques
et un décodage sans affichage côté GStreamer : ils ne valident pas
PostIt.Desktop Linux, ses périphériques réels ou son rendu Avalonia.
La validation multi-plateforme demeure incomplète.