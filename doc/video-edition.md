# Édition vidéo des directs et des replays

Document de travail à compléter ensemble. Les propositions ci-dessous ne sont
pas encore implémentées ; elles ne constituent pas toutes des décisions.

## Objectifs et décisions déjà prises

- Proposer **avant le début d'une émission** une option de raffinage de son
  replay. Sans cette option, conserver la finalisation automatique actuelle.
- Après le direct, permettre à l'auteur de placer des marques temporelles,
  d'apercevoir l'image correspondant à la position du curseur et de supprimer
  des plages délimitées par ces marques.
- Réaliser les découpes à l'image près, avec réencodage si nécessaire.
- Commencer par **PostIt desktop** (lecteur LibVLC et contrôles Avalonia),
  pour le raffinage comme pour la composition live. Android suivra, en
  réutilisant le modèle, l'API et le traitement serveur.
- Conserver les segments originaux d'une émission raffinée, mais en réserver
  l'accès à son auteur après la fin du direct.
- Envisager des génériques et incrustations **visibles pendant le direct**,
  donc également présents dans l'enregistrement de ce direct. Les génériques
  pourront utiliser du texte, une image ou une vidéo ; les incrustations
  pourront comprendre du texte interprété en tant que Markdown, des images et
  des vidéos existantes.
- Le générique de début, s'il est configuré, est joué jusqu'à sa fin
  naturelle. Le générique de fin est joué à la fin programmée de l'émission ou
  plus tôt, sur commande de l'auteur. L'audio des génériques et des vidéos
  incrustées est mélangé au microphone.
- Une émission programmée se termine à son heure de fin, avec ou sans
  générique de fin. L'auteur peut repousser cette fin autant de fois qu'il le
  souhaite.
- Si un direct programmé s'interrompt avant son générique de fin, le serveur
  diffuse un message « Direct interrompu » à la place de l'image, pendant
  5 minutes par défaut. L'auteur peut régler cette durée, dans la limite de
  15 minutes. Ce message est enregistré dans le replay. Si PostIt se reconnecte
  avant la fin du délai, le direct reprend ; sinon, l'émission s'arrête
  simplement, sans générique de fin. Un direct immédiat n'a pas ce
  comportement.
- Les marques du raffinage servent de repères et deviennent des chapitres du
  replay publié, sauf si l'auteur les supprime, une à une ou toutes à la fois.
  Une marque située dans un passage supprimé disparaît. Elles permettent aussi
  d'attacher au replay des incrustations à durée définie, non prévues avant
  le direct.
- Une fois le MP4 publié, l'auteur peut à nouveau éditer le replay et le
  remplacer, tant que les originaux sont conservés.
- Avant publication, les auditeurs voient l'émission avec l'état « Replay en
  préparation ».
- Les brouillons et les originaux sont conservés 7 jours par défaut. L'auteur
  peut choisir une durée comprise entre 0 et 365 jours ; avec 0, ils sont
  supprimés dès la publication du MP4. Après publication, leur suppression
  explicite est proposée.

Le raffinage du replay et la composition du direct sont deux fonctionnalités
distinctes. Une image ajoutée au direct est déjà intégrée aux segments reçus ;
elle n'est pas modifiable ou retirable lors du raffinage. En revanche, les
incrustations ajoutées pendant le raffinage ne concernent que le replay.

## Situation actuelle

PostIt capture et envoie des segments MPEG-TS au serveur. Lorsqu'un direct est
terminé, le serveur prépare automatiquement un MP4, actuellement par copie des
flux sans réencodage. Il conserve également les segments. Le lecteur de replay
utilise un MP4 navigable et des requêtes authentifiées par plages d'octets.
Le lecteur de direct utilise les segments et rejoint le flux près du direct.

Sur ordinateur, les contrôles du lecteur sont des contrôles Avalonia. Sur
Android, la surface vidéo et ses commandes sont natives. Sur desktop, le
curseur de replay met à jour l'image pendant son déplacement (voir le résultat
du prototype ci-dessous) ; sur Android, la recherche n'est encore validée qu'au
relâchement.


Les captures PostIt desktop et Android n'utilisent pas le même pipeline média.

### Hors périmètre : émissions OBS

Une émission diffusée avec OBS est entièrement gérée par OBS : scènes,
génériques, incrustations et contenu final. Le raffinage du replay et la
composition live décrits ici ne s'appliquent pas à ce pipeline. L'option de
raffinage n'est pas proposée pour une émission OBS, et son replay conserve la
finalisation automatique actuelle.

Références : [installation et fonctionnement des podcasts](../INSTALL.md),
[contrôles du replay](../src/PostIt/PostIt/Controls/ReplayControls.axaml.cs),
[capture PostIt](../src/PostIt/PostIt/Services/PodcastBroadcaster.cs),
[préparation MP4](../src/Yavsc.Server/Services/FfmpegPodcastReplayRemuxer.cs).

## 1. Raffinage du replay

### Parcours proposé

1. L'auteur active « Raffiner le replay avant publication » lors de la
   préparation de l'émission ou avant le démarrage d'un direct immédiat
   depuis PostIt.
2. À la fin du direct, le serveur prépare une source navigable privée et ne
   publie pas encore le MP4 final. L'auteur peut revenir ultérieurement.
3. Dans un éditeur, l'auteur déplace le curseur et voit l'image correspondant
   à la position demandée. Il ajoute des marques et leur aperçu.
4. Il choisit deux marques pour définir une plage à supprimer. Plusieurs
   plages peuvent être sélectionnées, revues et retirées avant validation.
5. Il peut attacher à une marque une incrustation (texte Markdown, image ou
   vidéo existante) avec une durée définie, en la prévisualisant.
6. L'auteur confirme la finalisation. Le serveur produit le MP4 à partir des
   plages conservées et des incrustations, puis le rend accessible aux
   auditeurs une fois prêt. Une finalisation sans modification doit aussi
   être possible. Jusque-là, les auditeurs voient « Replay en préparation ».
7. Après publication, l'auteur peut rouvrir l'édition et publier une nouvelle
   version qui remplace la précédente, tant que les originaux sont conservés.

L'éditeur doit distinguer clairement la source originale de la durée estimée
du résultat. Les marques et les plages sont exprimées dans le temps de la
**source originale**, même après ajout ou retrait d'une autre suppression.
Les modifications ne changent pas la source.

### Marques, aperçu et précision

Proposition pour une première version :

- curseur, temps courant et durée, avec commandes clavier accessibles ;
- image courante mise à jour pendant le déplacement, et état visible si l'image
  demandée est encore en cours de chargement ;
- bouton d'ajout de marque, liste des marques ordonnées avec temps et miniature,
  et possibilité de les supprimer une à une ou toutes à la fois ;
- sélection explicite d'une marque de début et d'une marque de fin, puis
  affichage de la plage à supprimer ; possibilité d'annuler cette sélection ;
- plages non vides et comprises dans la durée réelle ; fusion des plages
  qui se chevauchent ou se touchent. Refuser une sélection qui supprimerait
  toute la vidéo et expliquer l'erreur ;
- marques nommables, utilisables comme repères de navigation et publiées
  comme chapitres du replay. Leur temps est converti vers la durée du MP4
  final ; une marque située dans une plage supprimée disparaît et ne produit
  pas de chapitre ;
- incrustation attachée à une marque : contenu, durée, position et taille.
  Signaler une incrustation dont la marque tombe dans une plage supprimée ou
  dont la durée dépasse la vidéo conservée.

L'aperçu doit correspondre à la position effectivement décodée, pas simplement
à la valeur du curseur. Les demandes de recherche rapides doivent être
coalescées ou annulées pour éviter qu'une ancienne image remplace la plus
récente. Il faut vérifier le comportement du lecteur en pause, sur desktop et
Android, avant de choisir entre recherche vidéo directe et extraction
d'images ; ne pas supposer que la recherche actuelle est précise à l'image.
Les miniatures sont dérivées de la source privée et ne doivent pas apparaître
dans un cache public.

**Résultat du prototype desktop** : la recherche vidéo directe suffit.
LibVLC 3, avec sa recherche précise par défaut (sans `--input-fast-seek`),
affiche en pause l'image exacte demandée, y compris entre deux images clés et
en reculant. Un test natif le vérifie sur une vidéo synthétique dont chaque
image a une luminance distincte ; il échoue avec `--input-fast-seek`, qui
retombe sur l'image clé. Pendant le glissement, `ReplayControls` ne garde que
la dernière position demandée et l'envoie au lecteur au plus toutes les 60 ms ;
le relâchement applique la position finale. Restent à mesurer la réactivité
sur un long replay distant (requêtes par plages) et le comportement sur
Android.

Les limites de suppression sont définies sur les images réellement décodées ;
l'interface devra préciser quelle image est conservée à chaque limite. Le
traitement doit couper aussi l'audio, rétablir des horodatages continus et
éviter les désynchronisations. La durée estimée ne remplace pas une mesure du
MP4 final. La faisabilité et le coût doivent être mesurés sur des sources à
cadence variable, sans audio et avec audio.

**Résultat du prototype serveur** : `ReplayEditPlan` convertit les plages
supprimées en plages conservées : il fusionne les plages qui se chevauchent ou
se touchent, les borne à la durée, refuse les plages vides ou hors vidéo, la
suppression totale et plus de 200 plages. `FfmpegPodcastReplayEditor` ouvre la
source une fois par plage conservée, avec une recherche précise
(`-ss`/`-to` en entrée), puis assemble les plages avec le filtre `concat` et
réencode en H.264/AAC avec `+faststart`. Une plage [début, fin) conserve les
images dont l'horodatage est dans l'intervalle ; l'audio est coupé aux mêmes
temps. Seuls des nombres formatés et le chemin de la source, généré par le
serveur, sont transmis à FFmpeg.

- `trim` suivi de `setpts=PTS-STARTPTS` a été écarté : avec FFmpeg 7.1, la
  durée de la dernière image est perdue et cette image disparaît du MP4.
- **Origine des temps** : LibVLC et le `-ss` d'entrée de FFmpeg mesurent tous
  deux le temps depuis le début du format, c'est-à-dire du premier flux, et non
  depuis la première image vidéo. Mesuré : avec une vidéo qui commence 0,52 s
  après l'audio, LibVLC affiche à 1 500 ms l'image vidéo de 980 ms. Les marques
  prises dans le lecteur sont donc directement utilisables par le serveur.
- Lorsque l'audio commence avant la vidéo (décalage d'encodage AAC, segments
  live), chaque plage garde ce léger décalage. La vidéo finale peut ainsi
  dépasser d'une image la somme des plages, sans désynchronisation.
- Le test serveur vérifie, sur une source synthétique avec audio, la liste
  exacte des images conservées, les durées audio et vidéo, `faststart` et le
  refus d'un résultat dépassant le quota, avec et sans décalage de départ.
- Restent à mesurer : le coût du réencodage sur un replay long, et les
  sources à cadence variable issues des captures PostIt.

### États et persistance

Proposition, à adapter aux états déjà exposés par l'API :

| Étape | Comportement |
| --- | --- |
| Direct | Les auditeurs autorisés regardent le flux ; l'option de raffinage est déjà enregistrée. |
| Préparation de la source | Les segments sont terminés ; le serveur prépare une source navigable privée. |
| Brouillon | L'auteur peut prévisualiser, enregistrer ses marques, plages et incrustations, puis revenir plus tard. Les auditeurs voient « Replay en préparation ». |
| Finalisation | Une version immuable du montage est traitée en arrière-plan ; l'avancement ou l'état est visible. |
| Publié | Le MP4 final est accessible aux auditeurs ; les originaux restent privés. L'auteur peut rouvrir un brouillon. |
| Réédition | Le MP4 publié reste servi jusqu'à la publication atomique de la nouvelle version. |
| Échec | Une erreur explicite est visible ; source et brouillon restent disponibles pour correction ou nouvel essai. |

Le brouillon doit survivre à la fermeture de PostIt et au redémarrage du
serveur. Les opérations de sauvegarde et de finalisation doivent être
idempotentes ou détecter les conflits entre deux clients. Une finalisation ne
doit pas publier un fichier partiel : écrire dans un fichier temporaire,
vérifier le résultat, puis publier atomiquement. La reprise après interruption,
le nettoyage des fichiers temporaires et la suppression du podcast doivent être
définis pour chaque état.

Un brouillon et les originaux de l'émission sont conservés 7 jours par défaut ;
la durée est un réglage de l'auteur, de 0 à 365 jours. Une tâche de fond
supprime les brouillons et originaux expirés et prévient l'auteur dans
l'interface. Avec une durée de 0 jour, ils sont supprimés dès la publication du
MP4. Le MP4 publié n'est pas concerné, mais il ne peut plus être
réédité ensuite. Après publication, PostIt propose explicitement de supprimer
le brouillon et les originaux.

Une émission interrompue doit rester récupérable par son auteur. Lorsqu'un
direct programmé s'arrête après le délai « Direct interrompu », ou qu'un
direct immédiat est interrompu, une émission optée pour le raffinage passe en
préparation de la source puis en brouillon, comme à une fin normale. Les
originaux ne doivent pas devenir publics à cause d'une interruption ou d'un
redémarrage.

### Accès et sécurité

- Avant la fin du direct, conserver l'accès normal au flux live.
- Après la fin ou l'interruption d'une émission optée pour le raffinage,
  réserver à l'auteur les segments originaux, la source navigable, les
  miniatures et le brouillon. L'appliquer côté serveur à **toutes** les
  routes de médias concernées, pas seulement à l'interface.
- Ne pas servir aux auditeurs le replay provisoire ni une ancienne version
  non raffinée. Le MP4 final seul est publié après validation.
- Contrôler à nouveau l'identité et l'autorisation sur chaque requête,
  y compris les requêtes de plages ; ne pas inclure de jeton dans les URL.
- Ne pas promettre l'effacement d'images déjà vues ou enregistrées par un
  auditeur pendant le direct. La conservation des originaux signifie aussi
  que l'auteur peut toujours les consulter.

Limiter les entrées, la taille et la durée des traitements, et ne jamais
construire une commande shell à partir des textes ou chemins fournis par les
utilisateurs. Vérifier les quotas et l'espace pour les segments, la source
privée, les fichiers temporaires et le MP4 final ; conserver les règles de
comptabilisation existantes ou documenter explicitement leur évolution.

## 2. Génériques et incrustations pendant le direct

### Besoin envisagé

- **Début** : si un générique de début est configuré, il est joué jusqu'à sa
  fin naturelle avant le contenu principal.
- **Fin** : le générique de fin, s'il est configuré, est lancé :
  - automatiquement à la fin programmée de l'émission ;
  - plus tôt, par un bouton « Générique de fin », visible seulement si ce
    générique est configuré.

  Un bouton « Repousser la fin » décale la fin programmée de l'émission, et
  donc le lancement automatique du générique s'il existe, d'une durée
  configurable (5 minutes par défaut). Il est disponible même sans générique
  de fin et peut être utilisé autant de fois que nécessaire.
- **Pendant** : afficher et retirer des textes Markdown, des images ou des
  vidéos existantes par-dessus la vidéo, selon les commandes de l'auteur.
- **Audio** : l'audio d'un générique ou d'une vidéo incrustée est mélangé au
  microphone.
- **Interruption** : si un direct programmé s'interrompt avant son générique
  de fin, le serveur diffuse un message « Direct interrompu » à la place de
  l'image, pendant une durée réglable par l'auteur, de 5 minutes par défaut et
  15 minutes au plus. Ce message est enregistré dans le replay. Si PostIt se
  reconnecte avant la fin du délai, le direct reprend ; sinon, l'émission
  s'arrête simplement, sans générique de fin. Un direct immédiat
  n'affiche pas ce message.

Les génériques et incrustations doivent être visibles par les auditeurs en
direct et enregistrés dans les segments. Leur rendu dans un futur MP4 découle
donc de la capture. L'interface devrait permettre une prévisualisation avant
diffusion et indiquer clairement ce qui est actuellement à l'antenne.

Pour le texte et les images, des propriétés telles que contenu, position,
taille, durée, transparence et ordre des couches sont envisageables. Pour
une vidéo de générique, il faudra définir formats acceptés, résolution,
cadence, présence d'audio et transition avec la caméra et le microphone.
Les médias fournis doivent être autorisés pour cet usage et contrôlés avant
leur lecture ; aucun service externe n'est requis par cette proposition.

### Pistes techniques à évaluer, non décidées

1. **Composition côté diffuseur PostIt** : intégrer les éléments avant
   l'encodage sur chaque plateforme. Android dispose déjà d'un pipeline
   graphique pour le flou, tandis que le chemin desktop est différent.
   Vérifier prévisualisation, audio, consommation et continuité des segments.
2. **Composition côté serveur** : examiner une chaîne de traitement live en
   aval de la réception des segments PostIt. Mesurer son impact sur la
   latence, les ressources et la disponibilité ; ne pas l'adopter sans
   prototype et limites explicites.

Le message « Direct interrompu » est produit par le serveur, puisque le
diffuseur a justement cessé d'émettre. Le serveur détecte l'absence de
nouveaux segments, génère des segments contenant le message et les insère
dans la playlist et l'enregistrement, avec des horodatages continus. À la
reconnexion de PostIt, les segments reçus reprennent la suite. Le seuil de
détection doit éviter de déclencher le message sur un simple retard réseau.

Les émissions OBS composent leurs propres scènes et ne sont pas concernées.
Une panne de composition ne doit pas produire silencieusement un direct
différent de celui annoncé à l'auteur.

## 3. Ordre de réalisation proposé

1. Valider le parcours, la politique de visibilité et les questions ci-dessous.
2. Prototyper sur desktop la navigation précise et l'aperçu pendant le
   glissement (fait, voir « Résultat du prototype desktop »), puis
   l'assemblage audio/vidéo précis côté serveur (fait, voir « Résultat du
   prototype serveur »).
3. Ajouter le brouillon privé et ses états, les autorisations, les limites et
   la reprise après incident, puis l'éditeur de marques et plages desktop.
4. Valider la finalisation sur de vrais médias synthétiques et conserver le
   comportement existant pour les émissions sans raffinage.
5. Porter l'éditeur sur Android (surface native), en réutilisant l'API et le
   traitement serveur.
6. Étudier la composition live, d'abord sur desktop puis sur Android, avant
   d'engager les génériques et les incrustations.

## 4. Vérifications attendues

- Sans option de raffinage, le direct et le replay suivent le parcours actuel.
- Avec l'option, aucun auditeur autre que l'auteur ne peut lire les originaux
  après le direct, y compris via playlist, segments, aperçu ou requêtes partielles.
- Les marques suivent correctement le curseur ; plusieurs suppressions,
  chevauchements, limites et suppression totale sont traités explicitement.
- Sur une vidéo de test à images identifiables, la première et la dernière
  image autour de chaque coupe, le son et la durée du MP4 sont vérifiés.
- La fermeture de l'application, une interruption réseau, un redémarrage
  serveur, une erreur FFmpeg et un quota insuffisant ne publient pas de
  fichier incomplet et ne perdent pas le brouillon.
- Une émission OBS n'expose pas l'option de raffinage et conserve la
  finalisation automatique actuelle.
- Générique de fin lancé à l'heure programmée, sur commande et après
  plusieurs « Repousser la fin » ; fin automatique sans générique.
- « Direct interrompu » produit par le serveur, borné à sa durée, présent dans
  le replay, et remplacé par le direct si PostIt se reconnecte à temps.
- Les marques conservées deviennent des chapitres aux bons temps du MP4 final.
- Une réédition remplace le MP4 publié sans interruption de service ; les
  brouillons et originaux expirés sont supprimés selon la durée choisie.
- Les tests couvrent d'abord desktop, puis la surface native Android lors du
  portage ; les fonctionnalités live nécessitent aussi des essais sur appareil
  et des mesures de latence et de charge.

## 5. Questions à préciser ensemble

Toutes les questions de conception posées jusqu'ici ont reçu une réponse.
Les points encore ouverts sont techniques et seront tranchés par les
prototypes : chaîne FFmpeg d'incrustation, détection d'une interruption côté
serveur et choix de composition live (PostIt ou serveur). L'aperçu à l'image
près sur desktop (recherche directe LibVLC) et la découpe côté serveur
(une entrée FFmpeg par plage conservée, puis `concat`) sont tranchés.
