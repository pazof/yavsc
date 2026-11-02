# Appels audio-vidéo PostIt — architecture cible

## Statut et périmètre

Ce document décrit l'architecture cible et l'implémentation commencée.
La préférence de confidentialité est persistée et éditable dans le profil.
La signalisation est intégrée au chat, désactivée par défaut. Un premier
transport natif **Android et Desktop Linux en IPv4 directe (LAN/VPN ou IP publique)** est raccordé :
captures Podcast existantes et SRTP direct, sans libwebrtc ni nouvelle
couche JNI WebRTC. Il ne réalise pas encore la cible Internet, TURN ou groupe.
Il complète la [topologie de PostIt](postit.md) et les
[mécanismes de notification existants](notifications.md).
La cible couvre :

- les appels audio-vidéo bidirectionnels entre deux utilisateurs ;
- un réglage utilisateur booléen persistant permettant de masquer son adresse
  IP aux autres participants en imposant TURN ;
- les appels de groupe ;
- les clients Browser, Android et Desktop.

## Première implémentation Android et Desktop Linux : SRTP direct IPv4

Les adresses IPv4 publiques directement assignees a une interface locale active
sont egalement disponibles, avec la meme validation unicast cote client,
media et signalisation serveur. Les adresses non specifiees, link-local,
multicast, reservees (240/4), IPv6 et les formes non canoniques sont refusees.
Il faut autoriser en entree les deux ports UDP consecutifs choisis et assurer
la joignabilite entre les pairs. Ce support n'ajoute ni ICE/TURN ni traversee
NAT automatique : l'adresse publique d'un routeur ne remplace pas une adresse
assignee a la machine. Le consentement explicite au partage d'IP et le refus
du mode direct pour les comptes masquant leur IP restent inchanges.

Cette tranche réutilise la capture caméra/microphone et les encodeurs des
Podcasts, mais **pas leur segmentation HLS ni leurs envois de fichiers** :

```text
PostIt A ----------- ChatHub authentifié / HTTPS ----------- PostIt B
                     invitation, consentement,
                     adresses et clés, fin

PostIt A ========== H.264/AAC, MPEG-TS sur SRTP/UDP ========== PostIt B
                         dans les deux sens
```

L'implémentation est dans
[LibVlcLanCallMedia](../../src/PostIt/PostIt/Services/LibVlcLanCallMedia.cs),
[LinuxLanCallVideo](../../src/PostIt/PostIt/Services/LinuxLanCallVideo.cs) et
[LanCallViewModel](../../src/PostIt/PostIt/ViewModels/Chat/LanCallViewModel.cs).
Android utilise
[AndroidLanCallMedia](../../src/PostIt/PostIt.Android/Services/AndroidLanCallMedia.cs)
avec les mêmes contrats et clés SRTP.
Les contrôles et les deux surfaces vidéo sont intégrés à la page du chat.
Il n'y a pas de service de salles supplémentaire, d'enregistrement,
de fichier média temporaire ou de moteur WebRTC Android ajouté.

### Activer et essayer

1. Sur le serveur, activer explicitement les deux options :

   ```json
   {
     "AudioVideoCalls": {
       "Enabled": true,
       "EnableLanCalls": true
     }
   }
   ```

   Les deux valeurs sont `false` par défaut. Les paramètres TURN ne sont
   pas nécessaires pour ce mode. Utiliser le ChatHub en **HTTPS** ;
   invitation, acceptation et échange de clés LAN sont refusés en HTTP.
   Derrière un proxy TLS, configurer correctement le schéma HTTPS transmis
   à ASP.NET Core, uniquement depuis les proxys de confiance.
2. Utiliser deux clients Android 8+ ou Desktop Linux. Linux nécessite LibVLC
   avec V4L2, PulseAudio, x264, AAC et RTP/SRTP installés. Android réutilise
   la bibliothèque VLC embarquée pour les Podcasts et MediaRecorder pour
   H.264/AAC. La version Debian 13 a été utilisée pour les essais Linux.
   Les deux comptes doivent être présents dans
   la même salle existante et ne pas se bloquer mutuellement.
3. Dans « Appel direct IPv4 (LAN/VPN ou IP publique) », actualiser les périphériques,
   sélectionner la caméra, le microphone et une IPv4 privée locale
   accessible depuis le pair. Les adresses RFC1918 et de bouclage sont
   acceptées ; pas de DNS, IPv6 ou adresse publique. Le bouclage sert aux
   essais sur un même hôte, pas à joindre un second poste.
4. Choisir un port UDP **pair entre 1024 et 65534**, par défaut 5004.
   Autoriser l'entrée UDP sur ce port **et le suivant** depuis le pair dans
   le pare-feu : RTP sur le port pair, RTCP sur le port impair suivant.
   Les deux ports doivent être disponibles. Deux clients sur la même
   adresse doivent utiliser des paires distinctes.
   Aucun pare-feu n'est modifié automatiquement.
5. Saisir le pseudo exact du destinataire, confirmer la divulgation de
   l'adresse IPv4 locale (LAN/VPN ou publique), puis appeler. Le destinataire choisit ses propres
   périphériques et son adresse, confirme et accepte explicitement.
   Aucun accès à la caméra/microphone ne commence avant l'acceptation ;
   l'émission attend également la destination validée du pair.
6. Utiliser un **casque** : il n'y a pas d'annulation d'écho acoustique.
   « Refuser / Raccrocher », la sortie de page et la perte de SignalR
   arrêtent et libèrent les médias. Il n'y a pas de reprise automatique
   de capture après reconnexion.

### Capture et cycle de vie Android

- La caméra avant est sélectionnée en priorité dans la liste ; une autre
  caméra reste sélectionnable explicitement. Le microphone est obligatoire.
- Les permissions caméra, microphone et notifications sont demandées
  après acceptation et réception de l'endpoint distant, avant la capture.
  Un refus termine explicitement l'appel, sans tentative de capture de repli.
- Camera2, MediaRecorder et la rotation EGL sont ceux des Podcasts.
  Le descripteur du pipe MPEG-TS est lu directement par LibVLC (`fd://`)
  puis remultiplexé vers SRTP, **sans réencodage**, HLS ou fichier média.
  Le pipe système assure une mémoire bornée ; il n'y a pas de file de
  segments ni de lecture de capture par callbacks managés.
- La prévisualisation réutilise la surface EGL existante. Le retour
  distant utilise la surface Android LibVLC déjà employée pour les Podcasts.
- Le service de capture partagé affiche une notification d'appel avec
  l'action « Raccrocher » ; cette action ne publie aucun Podcast.
  Un Podcast et un appel ne peuvent pas posséder la capture simultanément.
- Le passage de PostIt en arrière-plan termine cet appel vidéo ; revenir
  dans l'application ne le redémarre pas. La sortie de page et la perte
  SignalR conservent la même règle d'arrêt.
- À l'arrêt, le lecteur natif libère d'abord son entrée. Le pipe restant
  est drainé pendant la finalisation de MediaRecorder, pour ne pas bloquer
  son écrivain, puis caméra, microphone, lecteurs et service sont libérés.

#### Particularité VLC Android : RTCP séparé

L'essai matériel initial a identifié un SIGSEGV dans `libc.__vfprintf`,
appelé par `vlc_memstream_vprintf` puis `sdp_AddAttribute` pendant la
création de la sortie RTP. Le code VLC 3 génère l'attribut sans valeur
`rtcp-mux` avec un format nul ; cette combinaison échoue avec le binaire
Android embarqué. Ce crash est distinct du précédent essai libwebrtc/JNI.

Les appels utilisent donc une paire RTP/RTCP standard, sans `rtcp-mux`,
sur Android **et** Linux. Les médias conservent les mêmes codecs, clés et
port RTP : c'est un changement de configuration du transport, pas
l'ajout d'un moteur WebRTC ou d'une cryptographie propre au projet.

### Confidentialité et limites

- Une clé SRTP aléatoire de 128 bits et un sel de 112 bits sont générés
  par appel et par direction avec le générateur cryptographique système.
  Seuls les deux appareils ayant accepté échangent un endpoint chacun
  via SignalR authentifié. Les représentations diagnostiques masquent
  adresse, clé et sel.
- Le chiffrement SRTP est fourni par LibVLC ; les clés passent par
  le serveur de signalisation, qui **peut donc les connaître**. Ce n'est
  pas un chiffrement de bout en bout à l'insu du serveur.
- Si l'un des comptes impose `HideIpAddressInCalls`, le mode LAN est
  **refusé**, y compris si la préférence change avant l'acceptation.
  Il n'y a aucun repli direct contournant cette préférence.
- Pas d'ICE, de STUN, de TURN, de traversée NAT automatique, de groupe,
  d'interopérabilité Browser, de négociation de codecs ni
  d'adaptation de débit. Ce mode exige une connectivite IPv4 directe entre les pairs
  (LAN/VPN ou adresse publique assignee), avec les ports UDP ouverts.
- H.264 à 30 images/s, cible 1 200 kbit/s ; AAC mono 48 kHz, 96 kbit/s.
  Ces réglages ne constituent pas une garantie de latence ni de débit.
  Le retour local n'émet pas le son du microphone.
- L'absence de première réception audio **et** vidéo provoque l'arrêt
  après 30 secondes ; les erreurs natives sont affichées et terminent
  l'appel. Après réception initiale, les compteurs LibVLC ne permettent
  pas ici de diagnostiquer précisément une panne média silencieuse :
  l'utilisateur peut raccrocher, et la perte de SignalR arrête toujours
  la capture. Pas encore de mesure de qualité ou de latence affichée.

### Validation effectuée et restante

Les tests natifs utilisent deux moteurs LibVLC réels, un fichier audio-vidéo
synthétique fourni explicitement et de vrais sockets UDP de bouclage :

- décodage bidirectionnel de trames vidéo et d'échantillons audio PCM ;
- refus de décodage avec une mauvaise clé, sans casser l'autre direction ;
- même échange avec la branche de prévisualisation locale activée
  (sortie factice dans ce test sans fenêtre).
- entrée MPEG-TS déjà encodée à travers un pipe, sans transcodage.

Les tests de signalisation couvrent consentement, sélection d'un seul
appareil, confidentialité, HTTPS, validation des endpoints et arrêt.
Les tests Avalonia sans fenêtre couvrent l'ordre invitation/acceptation/
capture, le refus, les événements tardifs, l'interruption, les erreurs média
et l'annulation d'un démarrage natif en attente. Ils vérifient aussi les
hooks Android, le choix initial de caméra avant, la configuration lors
d'une invitation entrante et l'arrêt lors du passage en arrière-plan.

Pour exécuter les tests média, définir `POSTIT_TEST_MEDIA` avec le chemin
d'un MP4 H.264/AAC de test (640 × 480), puis :

```sh
dotnet test --project src/PostIt/PostIt.Tests/PostIt.Tests.csproj \
  --filter-class '*LinuxLanCallMediaTests'
```

Sans cette entrée explicite, ces tests sont ignorés, pas remplacés par un
succès fictif. Les tests de contrôle s'exécutent sans caméra.
Le test de pipe utilise `POSTIT_TEST_TS`, un MPEG-TS H.264/AAC explicite.

Une activité **Debug uniquement**,
[AndroidLanCallTestActivity](../../src/PostIt/PostIt.Tests/AndroidLanCallTestActivity.cs),
teste l'adaptateur réel sur téléphone : caméra avant/microphone, surfaces
de retour et prévisualisation attachées, deux moteurs SRTP en UDP de
bouclage, retour synthétique et rejet d'une mauvaise clé. Elle n'utilise
ni compte ni API ; elle ne valide donc pas à elle seule un appel SignalR
entre deux comptes. L'activité n'est pas compilée dans l'APK Release.

Pour la lancer sur un appareil de développement dont les permissions
ont été accordées, installer l'APK Debug et ses assemblies via
`dotnet build -t:Install`, puis copier un MPEG-TS synthétique de 40 secondes,
640 × 480 avec audio AAC, sous `files/lan-call-fixture.ts` dans les données
privées de l'application. Exécuter :

```sh
adb shell am start -S -n fr.pschneider.postit/fr.pschneider.postit.LanCallTestActivity
adb shell run-as fr.pschneider.postit cat files/lan-call-test-result.txt
# Variante : ajouter --es wrongKey true à la commande am start.
```

Le résultat ne devient `PASS` qu'après vérification des médias et nettoyage
complet. Le test ne produit aucun enregistrement de caméra.
Sur le téléphone Android 15 arm64 connecté, l'essai positif a décodé
158 trames caméra et 399 360 échantillons audio en environ dix secondes,
avec un retour audio/vidéo décodé simultanément. L'essai avec mauvaise clé
a produit zéro trame et zéro échantillon dans la direction concernée,
tout en conservant le retour. Les deux essais ont terminé leur nettoyage,
sans service de capture restant. La capture Podcast antérieure a également
été retestée sur caméra avant : quatre segments, environ 11,8 secondes,
audio AAC présent, capture en arrière-plan puis arrêt par notification.

**La caméra physique, le rendu natif visible dans deux applications, le
son audible et un appel entre deux machines restent à valider côté Linux** :
aucune caméra n'était disponible sur cet hôte. La capture matérielle Android
est testée, mais un appel Android ↔ Linux ou entre deux téléphones distincts
sur LAN/VPN et la qualité acoustique restent à valider. Les essais de
bouclage ne prouvent pas une latence conversationnelle.

## Choix des protocoles

La table suivante demeure la **cible multi-plateforme initiale**, distincte
du premier transport Linux LAN/VPN décrit ci-dessus.

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

### Travail restant pour la cible Internet multi-plateforme

- Android : l'approche Java/JNI libwebrtc a été écartée à la demande du
  projet ; aucun adaptateur libwebrtc expérimental n'est conservé.
  L'adaptateur SRTP LAN réutilise désormais la capture Podcast ; la solution
  compatible avec la cible Internet reste à choisir.
- Desktop Linux : le transport SRTP LAN est raccordé, mais ne remplace pas
  les validations matérielles ni une solution de connectivité Internet.
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