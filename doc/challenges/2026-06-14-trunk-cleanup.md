# Défi : aligner `main` sur l'intention trunk + nettoyage CSS/JS

> Énoncé : 2026-06-14, par Paul Schneider.
> Statut : **ouvert**.

## Énoncé

Atteindre, sur la branche `main` et en mode trunk, l'état
intentionnel qui a déjà opéré sur la branche `themeok` (la
référence). Concrètement :

1. **Libérer le dépôt de son code inutile** : retirer les assets
   vendor non utilisés (Quill, dropzone.min.css, jquery-ui si plus
   personne ne s'en sert, etc.) et tout ce qui n'est référencé
   nulle part dans le code applicatif.

2. **Bien nettoyer le CSS et le JS** : déplacer les vendor libs
   sous un régime cohérent (npm + esbuild), arrêter le
   double-tracking (fichiers versionnés + bundles esbuild), poser
   des conventions claires.

3. **À minima, ne pas casser** le JS et le CSS de
   `_Layout.cshtml`. C'est l'invariant non-négociable.

4. **Introduire immédiatement** des tests d'UI automatisés pour
   servir de garde-fou contre la régression, à chaque commit.

## Pourquoi maintenant

`themeok` est devenu, dans la pratique, la branche de référence
pour le look et le comportement front. `main` n'a pas reçu les
commits correctifs de `themeok`, et son état est plus dégradé
(caches navigateur, état runtime, scripts inline qui plantent sur
du contenu vide).

Tant que la divergence existe, on risque de :
- cherry-picker à la main en ratant un commit,
- ne pas voir une régression front parce qu'on ne teste pas en
  navigateur,
- accumuler de la dette visuelle sur `main` jusqu'à un point de
  non-retour.

## État actuel

### `themeok` (référence, à 4 commits devant `main`)

```
ec8fd4b6 fix(cookies): set Identity cookies to SameSite=Lax in dev
76fef567 chore: remove Quill
379413f3 chore: snapshot of themeok branch as visual reference
d71490f5 chore: stop tracking vendor lib files
31906a78 WIP : Js and css cleanup      ← point de départ du WIP
30587248                                ← ancêtre commun avec main
```

État du working tree : propre. `dotnet test` : 11/11 verts.
Serveur dev local démarre sans warning (à part le certificat
HTTPS de dev non trusted, sans gravité).

### `main` (à remettre au niveau)

```
2a9760ca docs: roadmap à jour, archive TODO.fr
8f768240 WIP: refactoring pour déploiement
30587248                                ← même ancêtre commun
```

État runtime connu : **look catastrophique** sur
`http://localhost:5000`. Diagnostic (cf.
[§ Résultats du diagnostic](#résultats-du-diagnostic)) :
- Le HTML est rendu correctement, toutes les CSS et JS sont
  servies en HTTP 200.
- Le "look catastrophique" vient du **contenu vide** (Home/Index
  n'a aucune section) et d'une **erreur JS** dans un script
  inline qui plante quand le carousel est vide.

### `refac/js-bundle` (suspendue)

```
fad7fc8d refac: remove jQuery + jQuery UI imports from datetime
79658c84 refac: remove vendor imports from chat entry
6455178b refac: load jQuery + Bootstrap as global scripts in _Layout
ed7522c5 fix: drop nuget bootstrap and popper.js, npm bundles
196f4b0b chore: gitignore node_modules, build/, wwwroot/lib
```

Trois commits de migration jQuery-en-script-global. **Pas encore
fusionnés dans `themeok` ni dans `main`**. À intégrer dans le
plan, mais pas en première priorité.

## Résultats du diagnostic

Reproduit en worktree `~/Workspace/yavsc-main/` sur branche
`diag/main` (basée sur `main@2a9760ca`).

### Procédure

```bash
git worktree add -b diag/main ../yavsc-main main
cp ~/Workspace/yavsc/.env ~/Workspace/yavsc-main/.env
# Forcer le port : appsettings de main contient
# "Kestrel.Endpoints.Http.Url": "http://localhost:3002" qui entre
# en conflit avec le port 5000 du dev settings. Override par env :
cd ~/Workspace/yavsc-main/src/Yavsc.Org
Kestrel__Endpoints__Http__Url=http://localhost:5060 \
Kestrel__Endpoints__Https__Url=https://localhost:5061 \
ConnectionStrings__YavscConnection="Server=localhost;Port=5432;Database=yavscdev;Username=yavscdev;Password=***" \
ASPNETCORE_ENVIRONMENT=Development \
dotnet bin/Debug/net10.0/Yavsc.Org.dll
```

> **Note de Paul** : la procédure d'install / dev principale
> est en fait `cd contrib; make reinstall` (qui build en
> Release, copie vers `/srv/www/yavsc/`, et gere le service
> systemd `yavscOrg` sur port 3002). Le `dotnet run` direct
> n'est qu'un raccourci de debug. Le diagnostic ci-dessus a
> été fait en `dotnet run` sur un port libre (5060) parce que
> le port 3002 etait deja occupe par le service systemd
> existant.

### Fichiers servis (HTTP 200, taille correcte)

| URL                                              | Code | Taille  |
|--------------------------------------------------|------|---------|
| `/lib/jquery-ui/jquery-ui.min.css`               | 200  | 30770   |
| `/lib/bootstrap.quartz.min.css`                  | 200  | 244113  |
| `/css/site.css`                                  | 200  | 3009    |
| `/lib/jquery-ui/external/jquery/jquery.js`       | 200  | 285314  |
| `/lib/jquery-ui/jquery-ui.js`                    | 200  | 521054  |
| `/lib/bootstrap/dist/js/bootstrap.bundle.min.js` | 200  | 80721   |
| `/js/site.js`                                    | 200  | 364     |
| `/lib/bootstrap/js/carousel.js`                  | 200  | 7141    |

### HTML rendu (extrait du `<body>`)

```html
<nav class="navbar navbar-expand-sm navbar-dark bg-dark">...</nav>
<div id="myCarousel" class="carousel slide" data-ride="carousel" data-interval="6000">
    <div class="carousel-inner" role="listbox">
    </div>                                              <!-- VIDE -->
</div>
<div class="container py-4">
                                                        <!-- VIDE -->
</div>
<footer class="border-top footer text-muted">
    <div class="container">&copy; WTFPL 2025 ...</div>
</footer>
<script>
    $(document).ready(function(){
       var car = $('.carousel').first();
       var fi = car.children('.carousel-inner').first()
       .children('.item').first();                       <!-- null : plante -->
       var capt = fi.children('.carousel-caption-s');
       fi.addClass('active');
       car.animate({...});
    })
</script>
```

### Cause racine

1. Le HTML est structurellement correct : navbar Bootstrap 5
   `bg-dark`, carousel Bootstrap 4 (`data-ride`), container
   Bootstrap. Toutes les CSS et JS sont chargées.

2. Le **carousel est vide** (`<div class="carousel-inner">` sans
   enfant `.item`). Le script inline
   `car.children('.carousel-inner').first().children('.item').first()`
   retourne `null` → `fi.children(...)` lève
   `TypeError: Cannot read properties of null` → erreur console
   qui stoppe la chaîne d'initialisation.

3. Le **container principal est vide** : aucune section de
   contenu → page visuellement blanche malgré le BG art.

### Diff `main` vs `themeok` (fichiers versionnés)

| Fichier                                        | Diff        |
|------------------------------------------------|-------------|
| `_Layout.cshtml`                               | identique   |
| `_Nav.cshtml`                                  | identique   |
| `site.css`                                     | identique   |
| `_Layout.cshtml.css` (Razor scoped)            | identique   |
| `appsettings-org.Development.json`             | identique   |
| `appsettings-org.json` (production)            | `Kestrel.Endpoints.Http.Url` = `3002` sur main, absent sur themeok |

Donc **structurellement, `main` et `themeok` servent le même
HTML** quand on les lance en dev. La différence visible est due
au contenu des vues, pas aux fichiers de structure.

## Critères d'acceptation

Le défi est résolu quand :

- [ ] **CA1** : `main` contient les 4 commits de `themeok`
  (`d71490f5`, `379413f3`, `76fef567`, `ec8fd4b6`) — par merge ou
  cherry-pick.
- [ ] **CA2** : la branche `refac/js-bundle` est mergée (ou
  re-intégrée) sur `main`, via un commit explicite (pas un
  merge fast-forward silencieux).
- [ ] **CA3** : un test d'UI automatisé (Selenium Driver) existe et
  passe sur `main` ET sur `themeok`. Le test vérifie au minimum :
  - la page d'accueil répond 200,
  - le HTML contient la navbar Bootstrap,
  - la CSS `bootstrap.quartz.min.css` est chargée sans erreur,
  - aucun script `<script src>` ne retourne 404.
- [ ] **CA4** : la liste des vendor libs effectivement utilisées
  par le code applicatif est documentée (un fichier
  `doc/vendor-libs.md` ou équivalent). Tout fichier dans
  `wwwroot/lib/` non listé est candidat à la suppression.
- [ ] **CA5** : aucun script inline ne plante au chargement
  d'une page. Vérifié par le test d'UI (CA3) qui collecte les
  erreurs console.
- [ ] **CA6** : `Home/Index.cshtml` contient au moins une
  section visible (hero ou équivalent) — sinon le "look
  catastrophique" persiste même avec tout le reste correct.

## Stratégie de tests d'UI automatisés

Outil proposé : **Playwright** (C# / .NET, pour rester dans la
stack actuelle). À introduire en premier, **avant** les autres
commits, pour servir de filet de sécurité dès le début.

### Phase A — Bootstrap des tests UI

1. Créer un projet de test UI séparé :
   `test/yavsc.UiTests/yavsc.UiTests.csproj`
2. Dépendance : `Microsoft.Playwright` (NuGet).
3. Une classe `BaseUiTest` qui :
   - démarre le serveur sur un port dédié (5050 par convention),
   - configure Kestrel via `WebApplicationFactory<Program>`,
   - lance Playwright en mode headless.
4. Un premier test `HomePage_ShouldRender` qui vérifie CA3.

### Phase B — Extension de la couverture

Tests à ajouter au fil de l'eau, un par commit :
- `HomePage_Navbar_IsBootstrapDark`
- `HomePage_NoConsoleErrors`
- `AccountPage_Login_AndLogout` (quand pertinent)
- `BlogListPage_Renders`

### Garde-fou

À chaque commit qui touche `Views/`, `wwwroot/`, ou
`HostingExtensions.cs`, **les tests UI doivent passer**. C'est
la règle. Si on n'a pas le temps de migrer toute la suite de
tests, on ajoute au moins un test qui couvre le changement.

## Plan d'attaque

### Étape 0 — Tests UI (garde-fou immédiat)

Avant tout cherry-pick, mettre en place le projet de tests UI et
le premier test CA3. C'est le filet.

### Étape 1 — Cherry-pick des commits `themeok` sur `main`

Dans l'ordre chronologique, en testant à chaque commit :
- `d71490f5` (gitignore)
- `379413f3` (snapshot, vide)
- `76fef567` (Quill)
- `ec8fd4b6` (cookies)

OU un merge `themeok → main` (plus risqué si `main` a divergé
entre temps, mais ici c'est le bon choix car les 4 commits sont
consécutifs et bien isolés).

### Étape 2 — Nettoyage de fond

- Lister les vendor libs (`wwwroot/lib/`) réellement utilisées
  par le code (grep `src/Yavsc.Org/`).
- Documenter dans `doc/vendor-libs.md`.
- Pour les libs inutilisées : `git rm` (trackées) ou
  documentation (non trackées sur `themeok`).
- Pour les libs utilisées : confirmer qu'elles sont dans
  `package.json` ou statiquement référencées par un bundle
  esbuild.

### Étape 3 — Fusion `refac/js-bundle` → `main`

Cherry-pick des 3 commits, ou merge de la branche. Vérifier que
`core.bundle.min.js` (et les autres bundles) se génèrent
correctement via `npm run build:js` après le merge.

### Étape 4 — Test runtime manuel

- Démarrer le serveur dev.
- Naviguer sur Home, About, Contact, Blogspot, Account/Login.
- Vérifier visuellement l'absence d'erreur console.
- Comparer avec l'état de référence sur `themeok`.

### Étape 5 — Remplir `Home/Index.cshtml` (optionnel)

Si on a le temps et le contenu, ajouter une section hero et une
section services, pour que la page d'accueil ne soit plus vide.

## Risques identifiés

### R1 — Cache navigateur trompeur

Après un `git checkout` rapide, le navigateur peut servir une
version cachée du bundle avec un hash obsolète. Toujours faire
`Ctrl+Shift+R` ou vider le cache avant de juger un look.

Mitigation : le test UI (CA3) charge en Playwright, qui ne cache
pas. Donc on a un verdict objectif.

### R2 — Double binding Kestrel : `main` veut 3002, dev settings forcent 5000

`appsettings-org.json` sur `main` contient
`"Kestrel": { "Endpoints": { "Http": { "Url": "http://localhost:3002" } } }`.
Le dev settings le merge avec 5000/5001, mais en prod-like (via
`make reinstall` + systemd), c'est 3002.

**Ce qu'on a en local** :
- **Service systemd `yavscOrg`** (user `yavsc`,
  `ExecStart=/srv/www/yavsc/Yavsc.Org`) : tourne sur **port
  3002**, sert le build Release publié. C'est ce que Paul
  utilise pour le dev runtime "proche prod".
- **`dotnet run` depuis VSCode** (ou worktree) :
  `ASPNETCORE_ENVIRONMENT=Development` → port **5000/5001** via
  `appsettings-org.Development.json`.

Les deux setups sont légitimes et cohabitent. Le test d'UI
automatique doit pouvoir tourner contre les deux.

**Décision à prendre** : doit-on garder cette divergence
main/themeok ou aligner ? Mon avis : aligner sur 5000/5001 partout
en dev, et garder 3002 pour la prod (ce qui est déjà le cas
implicitement).

### R3 — `.env` non versionné

Le `.env` contient des secrets SMTP/ANTHROPIC et n'est pas dans
git. Le copier manuellement sur un worktree est une opération
manuelle qu'on peut oublier.

Mitigation : `doc/install.md` doit documenter cette étape. À
réviser.

### R4 — Ports utilisés

`5000/5001` : dev Yavsc
`3001/3002` : autre service `yavsc` user (prod-like) sur la machine
`5050/5051`, `5060/5061` : ports libres utilisés pour diag/tests

À documenter pour ne pas se marcher dessus.

### R5 — Quiproquos de branches

On a déjà perdu du temps dans cette session à ne pas savoir
quelle branche était active. Règle : **toujours** vérifier
`git branch -a` et `git status` au début d'une session, et
l'écrire en mémo.

## Ce qui est déjà fait (récap session 2026-06-14)

- 4 commits sur `themeok` (cf. log ci-dessus)
- Worktree `~/Workspace/yavsc-main/` sur `diag/main` créé
- Diagnostic de `main` effectué, cause racine identifiée
- 11/11 tests `dotnet test` verts
- `dotnet build` 0 erreur

## À faire

### Court terme (prochaine session)

- [ ] **Phase 0a** : documenter `cd contrib; make reinstall` dans
      `doc/install.md` (s'il existe, à créer sinon) — c'est la
      procédure officielle de Paul, elle doit être la référence
- [ ] Phase 0b : bootstrap tests UI Playwright
- [ ] Phase 1 : cherry-pick ou merge des 4 commits sur `main`
- [ ] Phase 2 : `doc/vendor-libs.md`

### Moyen terme

- [ ] Phase 3 : fusion `refac/js-bundle`
- [ ] Phase 4 : test runtime manuel
- [ ] Phase 5 : remplir `Home/Index.cshtml` (si scope confirmé)

### Long terme (backlog)

- [ ] Auditer les autres vues internes pour migration Bootstrap 5
- [ ] Documenter la procédure d'install (incluant la copie
      manuelle du `.env`)
- [ ] Réviser `doc/install.md` si existant
- [ ] CI : intégrer les tests UI Playwright dans le pipeline

## Annexes

### Liens utiles

- `doc/Architecture.md` : architecture générale de Yavsc
- `ROADMAP.md` : roadmap produit
- `git log themeok` : historique de la branche de référence
- `git log refac/js-bundle` : branche de migration jQuery

### Historique de cette session

Voir `memory/2026-06-14.md` (mémo interne OpenClaw) pour le récit
détaillé, quiproquos compris. Ce document-ci est l'**énoncé
exécutif** ; le mémo est l'**historique de travail**.

### Environnement d'exécution (état au 2026-06-14)

Sur la machine de dev, **deux process Yavsc** peuvent tourner en
parallèle :

| Process                          | Port  | User    | Lance par                  | Build        |
|----------------------------------|-------|---------|----------------------------|--------------|
| Service systemd `yavscOrg`      | 3002  | `yavsc` | `cd contrib; make reinstall` | Release      |
| `dotnet run` / VSCode            | 5000  | `paul`  | VSCode ou CLI              | Debug        |

**Avant de lancer un nouveau `dotnet run`** :
1. `ss -ltn | grep -E ':5000|:5001|:3002|:3001'` : voir qui écoute
2. `ps -ef | grep Yavsc.Org` : voir les process
3. Si 5000 occupé par `paul` : OK, c'est ton dev
4. Si 5000 occupé par autre : **ne pas lancer**, choisir un
   autre port (5060+) ET modifier `appsettings-org.Development.json`
   du worktree pour pointer dessus (les vars d'env
   `Kestrel__Endpoints__*__Url` peuvent forcer le port)
5. **Ne jamais toucher au service systemd** sauf demande explicite
   de Paul
