# Stratégie de test

Yavsc utilise **xUnit** (`xunit.v3`) avec un mix d'unitaire pur
et d'intégration légère. Les projets de tests sont sous
`src/<projet>.Tests/` et consomment le scaffold partagé
`src/Yavsc.Tests.Shared/`.

## Vue d'ensemble

| Sujet | Document |
|---|---|
| Scaffold partagé (`WebHostFixture`, JWT de test, etc.) | [src/Yavsc.Tests.Shared/README.md](../src/Yavsc.Tests.Shared/README.md) |
| Convention des dossiers de tests | [Conventions](#conventions-des-dossiers-de-tests) |
| Driver EF Core en test | [EF Core en test](#ef-core-en-test) |
| Stubs d'authentification et de permissions | [Auth et permissions](#auth-et-permissions) |

## Conventions des dossiers de tests

Sous `src/<projet>.Tests/`, on trouve quatre dossiers de premier
niveau qui classifient les tests par intention :

| Dossier | Usage |
|---|---|
| `NonRegression/` | Régressions : un bug constaté, un test qui le détecte si on le réintroduit |
| `Mandatory/` | Tests bloquants : ils doivent passer avant tout merge |
| `Smoke/` | Smoke tests HTTP rapides, montent un host léger |
| `Controllers/` | Tests unitaires des contrôleurs (mock du service, assertions sur le mapping HTTP) |

Les `NonRegression` sont la cible par défaut quand on fixe un
bug : ils doivent être **rouges avant le fix, verts après**, et
continuer à **casser** si quelqu'un revert le fix. Pas de test
qui passe à vide.

## EF Core en test

Pour les tests qui ont besoin d'un `ApplicationDbContext`, on
utilise **`UseInMemoryDatabase`** avec un `InMemoryDatabaseRoot`
partagé au niveau de la fixture. Pas de SQLite, pas de Docker,
pas de mock du contexte : le service testé s'exécute contre
un vrai `DbContext` sur in-memory.

```csharp
private static readonly InMemoryDatabaseRoot _dbRoot = new();

var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase("Yavsc.Org.Tests.MyFixture", _dbRoot)
    .Options;
```

Le `InMemoryDatabaseRoot` partagé est important : sans lui, EF
crée un store indépendant par `DbContext` dans certaines
configurations, et un test qui seed + read sur deux contextes
voit un store vide. Le pattern est documenté dans
`BlogsWebServerFixture` ([src/Yavsc.Blogs.Tests/BlogsWebServerFixture.cs](../src/Yavsc.Blogs.Tests/BlogsWebServerFixture.cs)).

> **Limite connue** : le provider in-memory **ignore** les
> `Migration` EF et ne respecte pas les FK **sur les raw
> SQL** (`ExecuteSqlRaw`). Pour tester des contraintes FK, on
> écrit la configuration dans `OnModelCreating` et on s'appuie
> sur le fait qu'EF la respecte à l'`Add`/`SaveChanges`. Pour
> tester des migrations, c'est l'environnement de staging.

## Auth et permissions

L'authorization policy provider de prod est swappé contre
`TestAuthPolicyProvider` (dans `Yavsc.Tests.Shared`) par les
fixtures spécialisées. Les tests qui ont besoin qu'un user soit
"Administrator" envoient un header `X-Test-Rôle` ; ceux qui
veulent un user anonyme omettent le header.

Pour les tests unitaires qui n'ont pas besoin du pipeline
HTTP, on stub `IAuthorizationService` directement (cf.
`BlogspotController` dans `Yavsc.Org.Tests/NonRegression/`)
pour éviter de monter un host complet.

## Tests cibles des podcasts

Avec le runner Microsoft.Testing.Platform du depot (.NET 10, xUnit v3) :

```bash
dotnet test --project src/test/Yavsc.Org.Tests/Yavsc.Org.Tests.csproj \
  --filter-class '*PodcastWebTests' --filter-class '*PerformerDonationTests'
dotnet test --project src/test/Yavsc.Api.Tests/Yavsc.Api.Tests.csproj \
  --filter-class '*LiveProcessorTests' --filter-class '*PodcastControllerTests' \
  --filter-class '*PodcastReplayTests' --filter-class '*PerformerDonationSettingsTests'
dotnet test --project src/PostIt/PostIt.Tests/PostIt.Tests.csproj \
  --filter-class '*PodcastTests' --filter-class '*PodcastReplayTests' \
  --filter-class '*YavscApiClientTests' --filter-class '*PodcastNativeCaptureTests' \
  --filter-class '*PerformerDonationSettingsTests'
dotnet test --project src/PostIt/PostIt.Tests/PostIt.Tests.csproj \
  --filter-method '*Shared_page_exposes_running_capture_preview_and_clears_it_before_disposal'
```

Les tests serveur couvrent HTTP/authentification, proprietaire, sequences,
reprises idempotentes, limites exactes, concurrence et persistance des replays
apres interruption/redemarrage, dans des dossiers temporaires isoles.
Les tests de configuration PayPal couvrent la lecture privee, la creation,
la validation avant toute sauvegarde, l'effacement, la compatibilite des anciens
clients, ainsi que le chargement, la liaison du champ et l'enregistrement PostIt.
Installer FFmpeg 5+ et ffprobe pour ces tests serveur : ils generent une
source H.264/AAC synthetique de quatre secondes, verifient le remuxage reel
(pistes, duree, faststart, decodage et seek a deux secondes), puis les reponses
HTTP authentifiees GET/HEAD/206/416 du worker. Aucune camera ni microphone
n'est utilise. Les tests client couvrent les plages exactes et bornees,
les seeks avant/arriere/EOF, l'annulation, le renouvellement des jetons
et le seuil exact d'auto-hide de trois secondes.
Pour tester aussi le decodage MP4 et les seeks avant/arriere dans LibVLC,
definir `POSTIT_TEST_REPLAY` sur un MP4 H.264/AAC local de 8 a 30 secondes
(LibVLC installe sur la machine de test). Le test natif est ignore sinon.
Verifier manuellement les overlays sur Desktop et Android : pause/reprise,
glissement de la barre, masquage apres trois secondes, reapparition au
toucher/survol et nettoyage apres navigation ou mise en arriere-plan.

Les tests natifs sont explicitement ignores sans `POSTIT_TEST_MEDIA`.
Pour les activer, installer LibVLC avec ses encodeurs H.264/AAC et definir
cette variable sur un fichier video local d'environ huit secondes contenant
une piste audio. Ils exercent la segmentation LibVLC reelle, l'envoi et la
reprise apres acquittement perdu, le dernier segment apres arret et le
decodage authentifie du replay MPEG-TS. FFmpeg n'est pas une dependance
d'execution de PostIt (le serveur MP4, lui, en a besoin).
Les tests materiels camera/microphone Windows/Linux restent manuels.
Les deux variantes du test d'encodage HLS (avec/sans apercu) necessitent
egalement `ffprobe` pour verifier independamment les pistes H.264 et AAC
du premier segment. La variante avec apercu attend une image decodee
par LibVLC dans un tampon memoire, sans ouvrir de camera ni diffuser le son
sur les haut-parleurs. Le test de page headless verifie aussi le binding
du controle d'apercu et son retrait avant liberation de la capture.

### Rotation de la capture Android

Le calcul capteur/ecran/camera avant et les quatre transformations de texture
sont couverts sans appareil :

```bash
dotnet test --project src/PostIt/PostIt.Tests/PostIt.Tests.csproj \
  --filter-class '*CameraFrameOrientationTests' --filter-class '*CameraFrameClockTests'
```

Un test Android supplementaire verifie le chemin OpenGL et l'encodeur reels,
sans ouvrir la camera, sans microphone et sans contacter l'API :

```bash
dotnet build src/PostIt/PostIt.Android/PostIt.Android.csproj -c Debug \
  -p:EmbedAssembliesIntoApk=true
adb install -r -t src/PostIt/PostIt.Android/bin/Debug/net10.0-android/fr.pschneider.postit-Signed.apk
adb shell am start -n fr.pschneider.postit/fr.pschneider.postit.CameraFrameRotationTestActivity
adb shell run-as fr.pschneider.postit cat files/camera-rotation-test-result.txt
```

Attendre la fin du test : `RUNNING` doit devenir `PASS`. Il encode quatre
quadrants colores en MPEG-TS pour 0, 90, 180 et 270 degres, decode une image et
verifie les dimensions (640x480 ou 480x640) et les couleurs aux quatre positions.
La tolerance de chaque composante RGB est de 40 pour tenir compte de H.264.
Il verifie aussi les couleurs RGBA de l'apercu OpenGL (tolerance 5), les
bandes noires sans deformation et le detachement/rattachement de sa surface
pendant l'encodage, toujours pour les quatre rotations.
Une erreur de rendu, d'encodage, de decodage ou de comparaison produit `FAIL`
avec l'exception. Cette activite est exclue des builds Release.

Le test synthetique ne couvre pas l'horloge Camera2 ni sa synchronisation avec
le microphone. Avec l'accord de la personne utilisant le telephone, lancer
aussi le test local reel (camera et microphone actifs, aucun envoi a l'API) :

```bash
adb shell am start -n fr.pschneider.postit/fr.pschneider.postit.PodcastCaptureTestActivity --es microphone true
adb shell run-as fr.pschneider.postit cat files/podcast-capture-test-result.txt
```

Attendre `PASS` : au moins trois segments, 8 a 16 secondes capturees,
pistes H.264 et AAC presentes, et progression des segments en arriere-plan.
Ce test detecte notamment les captures de zero seconde dues au decalage entre
l'horloge Camera2 et celle de MediaRecorder. Les fichiers restent dans le
repertoire prive indique par le resultat pour verification locale.
Le resultat inclut les angles capteur/ecran et la premiere matrice
SurfaceTexture recue. Les tests unitaires couvrent notamment la matrice
reelle `[0,-1,0,0,-1,0,0,0,0,0,1,0,1,1,0,1]` : pour un capteur a 90 degres
et un ecran a 0, elle porte deja la rotation et ne doit pas recevoir 90 degres
supplementaires. Le test synthetique seul ne detecte pas cette double rotation.

Verifier aussi manuellement les cameras avant/arriere en portrait et paysage,
le son, le direct et le replay, et la poursuite de l'emission en arriere-plan.
L'orientation est fixee au demarrage d'une emission ; tourner le telephone
pendant l'emission ne reconfigure pas le format encode.

## Quand ne PAS écrire de test

Un test qui ne détecte rien n'est pas un test. Si l'invariant
qu'on cherche à protéger est déjà enforced par EF, par le
compilateur, ou par une couche applicative en amont, le test
est du bruit. Mieux vaut :
- Un test qui assert un **comportement observable** (code
  retour HTTP, exception typée, valeur de retour)
- Ou pas de test, et une note dans le code

La non-régression se prouve par un test qui casse si on
réintroduit le bug. Pas par un test qui passe aujourd'hui et
qui continuera à passer après un revert.
