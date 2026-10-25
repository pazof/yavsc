# Appels audio-vidéo PostIt — architecture cible

## Statut et périmètre

Ce document décrit un **plan d'implémentation**, pas une fonctionnalité déjà
disponible. Il complète la [topologie de PostIt](postit.md) et les
[mécanismes de notification existants](notifications.md).

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
La signalisation peut utiliser l'infrastructure SignalR existante, mais les
contrats d'appel et leurs autorisations doivent être distincts du chat.
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

Prévoir un booléen de compte, nommé provisoirement `HideIpAddressInCalls`,
persisté côté serveur et accessible en lecture/écriture uniquement par
l'utilisateur concerné via la gestion de son profil. Le réglage est partagé
entre ses appareils ; une préférence locale seule ne suffit pas.

Valeur initiale proposée : `false`, à confirmer avant implémentation.
`false` autorise le chemin direct mais n'interdit pas TURN lorsqu'il est
nécessaire à la connectivité. `true` impose un transport relayé.

Présenter le réglage comme « Masquer mon adresse IP aux autres participants »,
en précisant le surcoût potentiel en latence et en bande passante serveur.
Il ne procure pas l'anonymat : les services contactés, notamment l'API, le
serveur de signalisation et TURN, peuvent toujours connaître l'adresse IP.

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