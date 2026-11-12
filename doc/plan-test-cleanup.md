# Plan de nettoyage de la suite PostIt

## Objectif

Rendre la suite PostIt plus rapide, plus fiable et plus lisible, sans bloquer le développement quotidien par des tests dépendants d’un matériel, d’un binaire externe ou d’un environnement local spécifique.

## Problème actuel

La suite a grossi sans vraie séparation entre :

- tests rapides et déterministes,
- tests UI headless,
- tests d’intégration,
- tests dépendants de l’environnement,
- tests Android / native / device-specific.

Résultat : la routine par défaut devient bruitée et peu fiable, et les sujets de fond (comportement métier) sont noyés dans des tests exigeants en dépendances système.

## Plan proposé

### 1. Séparer la suite par catégorie de dépendance

Les tests doivent être classés selon leur coût et leur dépendance externe, avec des annotations de classe xUnit du type :

```csharp
[Trait("Category", "Native")]
[Trait("Category", "Slow")]
public sealed class PodcastNativeCaptureTests
```

Et pour les tests Android :

```csharp
[Trait("Category", "Android")]
public class AndroidAppLaunchTests
```


- Fast unit tests
- Headless UI tests
- Integration tests
- Native / environment-dependent tests
- Android-only tests

La règle doit être simple : un test ne doit entrer dans la routine par défaut que s’il est exécutable sans matériel, binaire externe ni fichier de test local spécifique.

### 2. Garder la routine par défaut “portable”

La suite standard de développement doit couvrir uniquement :

- les comportements métier,
- les flux UI sans environnement spécifique,
- les tests reproductibles sans dépendances natives.

Les tests qui exigent :

- LibVLC,
- ffprobe / codecs,
- fichiers médias locaux,
- Android emulator / adb,
- services device-bound,

doivent être exclus de la routine standard.

### 3. Isoler les tests non-portables

Les tests dépendants de l’environnement doivent être déplacés ou séparés dans une catégorie spécifique, par exemple :

- `Native`
- `Android`
- `Slow`

Cela permet de conserver la signalisation de leur objectif sans polluer la suite principale.

### 4. Nettoyer les doublons fonctionnels

Les tests qui vérifient le même comportement avec seulement de légères variantes doivent être regroupés, fusionnés ou simplifiés.

Règle de qualité :

- 1 test = 1 comportement clair,
- 1 titre = 1 intention claire,
- 1 appel de fonction = 1 vérification utile.

### 5. Structurer les tests par feature et par niveau

Exemple de structure suggérée :

- `PostIt.Tests/Fast/`
- `PostIt.Tests/Headless/`
- `PostIt.Tests/Integration/`
- `PostIt.Tests/Native/`
- `PostIt.Tests/Android/`

Cette structure permet de distinguer immédiatement les tests de comportement des tests d’infrastructure.

### 6. Séparer la CI en deux niveaux

- CI standard / PR : tests rapides + headless
- CI nocturne / full : tests native + Android + integration lourde

Cela permet d’avoir un cycle de développement rapide sans sacrifier une vraie validation dans les scénarios spécifiques.

## Catégorisation cible des tests

Important : les catégories xUnit ne doivent être appliquées qu’aux vraies classes de test. Les `Activity` Android utilisées comme harness de test ne sont pas des classes de test et ne doivent pas porter de `[Trait("Category", ...)]`.

### Native

Classes de test xUnit qui dépendent de LibVLC, de fichiers multimédia locaux, de codecs ou d’un support natif Linux/OS spécifique.

- `LinuxLanCallMediaTests`
- `PodcastNativeCaptureTests`
- `PodcastReplayTests`
- `WebRtcCallViewModelTests` — sous-cas natif activé avec `POSTIT_TEST_NATIVE_WEBRTC=1`

### Android

Classes de test xUnit qui tournent sur un contexte Android spécifique, sur `adb`, sur un émulateur ou sur un device Android réel.

- `AndroidAppLaunchTests`

### Slow

Classes de test xUnit lourdes ou lentes, souvent partielles sur le flux média, la capture ou la lecture de données. Elles doivent être explicitement marquées comme `Slow` pour ne pas polluer la suite standard.

- `PodcastNativeCaptureTests`
- `PodcastReplayTests`
- `LinuxLanCallMediaTests`
- `WebRtcCallViewModelTests` — variante native / réseau / media-heavy

### Harness Android de support (non test classes)

Ces classes servent à déclencher un test Android dans un `Activity` dédié, mais elles ne sont pas des classes xUnit de test et ne portent pas de trait de catégorie.

- `AndroidCameraFrameRotationTestActivity`
- `AndroidChatServiceTestActivity`
- `AndroidLanCallTestActivity`
- `AndroidMarkdownEditorTestActivity`
- `AndroidPodcastBlurTestActivity`
- `AndroidPodcastCaptureTestActivity`
- `AndroidScreenAwakeTestActivity`
- `AndroidWebRtcTestActivity`

### Tests non-portables à sortir de la routine par défaut

Les classes de test ci-dessous dépendent d’un environnement local spécifique, d’un binaire externe ou d’un matériel Android / native. Elles ne devraient pas faire partie de la routine quotidienne standard.

- `AndroidAppLaunchTests`
- `LinuxLanCallMediaTests`
- `PodcastNativeCaptureTests`
- `PodcastReplayTests`
- `WebRtcCallViewModelTests` — cas native spécifique avec `POSTIT_TEST_NATIVE_WEBRTC=1`

## Exemples de dépendances qui rendent ces tests non-portables

- `POSTIT_TEST_MEDIA`
- `POSTIT_TEST_TS`
- `POSTIT_TEST_REPLAY`
- `POSTIT_TEST_NATIVE_WEBRTC`
- `adb` / appareil Android connecté
- LibVLC installé avec les codecs nécessaires
- fichiers vidéo/audio locaux
- machines Linux spécifiques

## État souhaité

Une suite PostIt propre doit avoir :

- une routine standard courte et stable,
- des tests natifs clairement isolés,
- une séparation nette des responsabilités,
- un signal de qualité lisible pour les contributeurs.
