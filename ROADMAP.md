# Yavsc — Roadmap

> **Statut** : document vivant. Reflète l'état du repo et les décisions en cours.
>
> L'ancienne liste en français (jalons 1–5, 2016–2017) est archivée dans
> [`TODO.fr.md.archive`](./TODO.fr.md.archive) — tout ce qui en a été réalisé
> ou reformulé est ici.

---

## 🧭 Vision

Yavsc est une plateforme de **mise en relation client / fournisseur / coordinateur**,
spécialisable par activité, avec une **origine musicale**.
Origine du nom : *Yet Another Very Small Company*.

Le domaine est modélisé en **DDD** (cf. [`doc/ddd-exploration-2026-06-14.md`](./doc/ddd-exploration-2026-06-14.md))
et l'architecture globale est décrite dans [`doc/Architecture.md`](./doc/Architecture.md).

Trois principes non négociables traversent tous les jalons :

1. **Devis, pas facturation directe.** Le flux contractuel prime sur le flux
   d'encaissement ; l'argent arrive seulement après accord signé.
2. **Open by design.** Le code des livrables et la plateforme sont sous licence
   libre ; les workflows métier aboutissent à des livrables sous licence libre.
3. **Pas de MVP borné.** Le périmètre est complet dès la conception — les
   jalons ne sont pas une découpe de fonctionnalités, mais une **séquence de
   stabilisation**.

---

## 🏗️ État de l'art (juin 2026)

### Périmètre technique

| Brique | Module | État |
|---|---|---|
| Backend ASP.NET | `Yavsc.Server`, `Yavsc.Api`, `Yavsc.Org` | actif, build OK, cible .NET 10 |
| Domaine partagé | `Yavsc.Abstract` | actif |
| Blogs | `Yavsc.Blogs` | actif |
| Client chat (`PostIt`) | `PostIt` (Avalonia) + `PostIt.Android` / `PostIt.Browser` / `PostIt.Desktop` | actif, OIDC |
| CLI admin | `src/cli` | actif |
| Tests | `test/Yavsc.Org.Tests` | présents, à densifier |
| Conteneurisation | `Dockerfile`, `Dockerfile.backend`, `docker-compose.yaml` | actif, image `pazof/yavsc-build-env` |
| CI | `.github/workflows/`, Dependabot (multi-ecosystems) | actif |
| Docs | `doc/Architecture.md`, `doc/ddd-exploration-2026-06-14.md` | frais (13–14 juin 2026) |

### Bounded Contexts identifiés

| # | BC | Module | Maturité |
|---|---|---|---|
| 1 | Chat | `PostIt` | client riche + backend, OIDC |
| 2 | Profil & Identité | `Yavsc.Org` | en place |
| 3 | Activités Légales | `Yavsc.Org` (à séparer ?) | taxonomie présente |
| 4 | Blogs | `Yavsc.Blogs` | en place |
| 5 | Prestation | `Yavsc.Server` | cœur métier, à compléter |
| 6 | Paiements | `Yavsc.Server` | PayPal NVP/SOAP, **arrhes vs avance** à clarifier |
| 7 | Rencontre (mode 2) | à créer | pas démarré |
| 8 | Modération | `Yavsc.Org` / à séparer | workflow 2 étages, amorcé |
| 9 | Trust & Safety | à séparer | blacklist plateforme, à industrialiser |
| 10 | Conciliation | à créer | jalon 5, pas démarré |

---

## 🚦 Jalons

> Convention : `☐` à faire · `◐` en cours · `✔` livré
>
> Chaque jalon a un **critère de sortie** vérifiable.

### Jalon 0 — Fondations techniques *(en cours)*

> Cible : pouvoir parler du domaine sans se battre avec le runtime.

- ✔ Conteneurisation de bout en bout (build env + run + compose) — Dockerfile multi-stage + `docker-compose.yaml` 4 services + healthchecks ; critère de sortie atteint : db/api/blogs démarrent sur une machine vierge, web documente sa dépendance au cert HTTPS (IdentityServer8 Production). Procédure d'install complète dans [CONTRIBUTING.md](./CONTRIBUTING.md#conteneurisation).
- ✔ Tests d'intégration smoke par BC — [src/Yavsc.Org.Tests/Smoke/](./src/Yavsc.Org.Tests/Smoke/) : `AccountSmokeTests` (`GET /signin`) + `BlogSmokeTests` (`GET /BlogSpot/Index`) couvrent deux BCs actives (Account, Blog front) en démarrant Yavsc.Org en mémoire via `WebApplicationFactory<Program>`. Coverage Yavsc.Api / Yavsc.Blogs reste à ajouter dans une session ultérieure.
- ✔ Découpage `Yavsc.Org` vs `Yavsc.Server` vs `Yavsc.Api` clarifié dans l'Architecture — [doc/architecture/decoupage-organisation.md](./doc/architecture/decoupage-organisation.md)
- ✔ `CONTRIBUTING.md` (build, tests, conventions, DDD sessions) — [CONTRIBUTING.md](./CONTRIBUTING.md)

**Critère de sortie** : `dotnet build` + `dotnet test` + `docker compose up` verts sur une machine vierge.

---

### Jalon 1 — Prestation signée de bout en bout

> Cible : un projet client/fournisseur aboutit à un **devis signé par les deux parties**, traçable, avec notifications.

Sous-jalons du `TODO.fr.md.archive` repris ici :

- ◐ **Devis validé fournisseur → client** : notification + mise à disposition du devis signé
- ☐ **Accord client sur devis** : notification fournisseur + mise à disposition signature client + commentaire
- ☐ **Édition facture non acquittée** depuis mobile
- ☐ **Édition contrat** depuis mobile
- ☐ **Notification & mise à disposition contrat** pour le client
- ☐ **Signature contrat par le client**
- ☐ **Notification facture non acquittée** au client
- ☐ **Paiement du client** (arrhes ? — voir jalon 2)
- ☐ **Édition facture marquée acquittée + signée** par le fournisseur
- ☐ **Notification facture acquittée signée** au client
- ☐ **Évaluation de la prestation** (des deux côtés)

**Critère de sortie** : scénario E2E web + mobile, devis → signature croisée → contrat → facture, sans intervention manuelle d'un opérateur.

---

### Jalon 2 — Espace personnel, blog opérationnel, marché ouvert

> Cible : un prestataire vit sur Yavsc (stocke, publie, est trouvé).

Repris du `TODO.fr.md.archive`, jalon 1 (sélection) + jalon 2 (interface) :

- ☐ **Gestion du profil utilisateur** Simple & Prestataire, sur **Android** (Web ✔)
- ☐ **Gestion de l'espace de stockage personnel** (Web, API web, Mobile)
- ☐ **Création d'une annonce**
- ☐ **Transformation annonce ↔ demande nominative**
- ☐ **Sélection d'une star** depuis le **mobile** (Web ✔)
- ☐ **Sélection DJ / chanteur / formation** (recherche par critères : date, lieu, tendance, taille, répertoire) depuis le **mobile**
- ☐ **Géocodeur de lieu** côté mobile
- ☐ **Géocodeur d'adresse postale** (Web + Mobile)
- ☐ **Évaluation des trajets**
- ☐ **Interface de gestion des collaborateurs** (sous-traitance — déjà amorcée dans `doc/Architecture.md`)
- ☐ **Migration de l'identifiant utilisateur** : `string` GUID → `long` auto-incrémenté (Npgsql)
- ☐ **Login with Twitter / PayPal**
- ☐ **Contrôle Web du rating**
- ☐ **Saisie et usage des disponibilités** (ponctuelles, récurrentes, congés)
- ☐ **Notifications structurelles** (impératives, par groupe)
- ☐ **Notifications aux posts, à l'arrivée d'un artiste, à une success story**
- ☐ **Construction du droit à l'envoi d'un message privé** (par destinataire, par accréditation, temporaire / définitif, par plage de temps, par validité d'un devis)
- ☐ **Paiement client du reste de la prestation**
- ☐ **Podcasts**
- ☐ **Personnalisation des blogs**
- ☐ **Monétisations**
- ☐ **Distribution de gold card / green card** (par un prestataire, donnant accès au chat privé)
- ☐ **Carte blanche** (délégation d'agenda, implique droit au chat privé)
- ☐ **Badges temporaires** (téléchargement unique, limité dans le temps, sans autre autorisation)

**Critère de sortie** : un utilisateur mobile peut publier une annonce, être sélectionné, signer, encaisser.

---

### Jalon 3 — Commande générique & formulaires extensibles

> Cible : le système de prise de commande n'est plus codé en dur par activité.

- ☐ **Saisie du devis sur commande générique** : les commandes sont des *projets utilisateur* associés à des *listes de formulaires* spécialisables
- ☐ **Groupe de champs** avec titre, contenant des champs :
  - ☐ Label
  - ☐ Type de valeur
  - ☐ Type de contrôle
  - ☐ Liste de validateurs
  - ☐ Invitation à la saisie
- ☐ **Saisie d'un devis à destination d'un invité** (envoi par email, contact local)
- ☐ **Paiement d'arrhes** (cf. décision arrhes vs avance à prendre en jalon 0/1)

**Critère de sortie** : ajouter une nouvelle activité (code APE, formulaire, contrôles) sans toucher au code du backend, par simple configuration.

---

### Jalon 4 — Rencontre, géolocalisation, streaming

> Cible : Yavsc devient aussi un lieu de rencontre (Mode 2), pas seulement un outil de travail.

- ☐ **Aide à la rencontre physique** : carte interactive, geofencing, alertes
- ☐ **Activités secondaires** : spécialisation d'une activité, activités filles, *activité principale* (code APE)
- ☐ **Streaming vidéo pair-à-pair**
- ☐ **Streaming vidéo public**

**Critère de sortie** : un événement géolocalisé notifie ses participants confirmés à l'approche du point de rendez-vous.

---

### Jalon 5 — Conciliation & réseau pro

> Cible : un conflit entre client et fournisseur a un chemin de résolution outillé.

- ☐ **Conciliation** : double vue client/fournisseur sur le dossier, indicateurs, *FrontOffice* tranche et propose un partage des torts
- ☐ **Réseau pro** : sous-traitance en cascade, une demande n'est validable que lorsque **tous les devis** dont elle dépend sont validés

**Critère de sortie** : un dossier en litige peut être instruit par un conciliateur sans échange d'emails en parallèle.

---

## 🔒 Hygiène transverse (à tous les jalons)

### Sécurité

- ☐ Quota filesystem par utilisateur
- ☐ Clés anti-forge sur toutes les interfaces Web
- ☐ Limites de taille des chaînes du modèle (là où ça manque)
- ☐ Revue des scopes OIDC (PostIt, API)

### Accessibilité

- ☐ Correction des libellés manquants / en anglais

### Réécritures techniques programmées

- ☐ Éviter la création de fichiers TeX intermédiaires pour la génération PDF
- ☐ Isolation `Yavsc.Api` (auth + rate limiting)

### Configurabilité

- ☐ Gestion de contenu des pages du site au format interne (non exposé au Web)
  - Cible : `@Html.SiteContent<AccessRules>(id).Responsible` (style à finaliser)
