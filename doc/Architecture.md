# Architecture de Yavsc

## Vision générale

Yavsc est une plateforme de mise en relation client/fournisseur,
spécialisable par domaine d'activité, avec une origine musicale.

Elle gère des **devis** (pas de facturation directe), et s'intègre
dans des workflows collaboratifs pouvant aboutir à des livrables
sous licence libre.

---

## Workflow de mise en relation multi-parties

### Initiateur

Le projet peut être initié par n'importe quelle partie :
- Un **client** (particulier ou pro) qui exprime un besoin
- Un **fournisseur** qui propose une offre ou monte un collectif
- Un **tiers coordinateur** qui orchestre sans être client ni prestataire

### Rôles

| Rôle          | Type de compte     | Description                                               |
|---------------|--------------------|-----------------------------------------------------------|
| Client        | Pro ou particulier | Exprime le besoin, valide les devis, co-signe la licence  |
| Fournisseur   | Pro                | Répond aux besoins, peut sous-traiter                     |
| Coordinateur  | Pro ou particulier | Orchestre le projet sans relation de facturation directe  |

### Sous-traitance

Un fournisseur peut faire appel à d'autres fournisseurs,
**avec accord explicite du client**. Chaque sous-traitant :
- Est visible dans le projet
- Co-signe l'accord de licence
- Peut recevoir un devis distinct

### B2B / B2C

La distinction est portée par le **type de compte** :
- Compte **pro** : SIRET, TVA, facturation professionnelle
- Compte **particulier** : usage personnel, sans obligations fiscales pro

Un projet peut mélanger les deux (ex: un particulier client,
plusieurs prestataires pro) — Yavsc n'impose pas d'homogénéité.

### États d'un projet

```
Initié → En recherche de parties → Devis en cours
       → Accord de licence signé → En production
       → Livré → Publié (si licence libre)
```

### Contrainte clé

L'accord de licence est établi **avant** tout début de production,
signé (ou validé) par toutes les parties : client, fournisseurs,
et sous-traitants éventuels.

---

## Domaine musical — Titres collaboratifs

### Objectif

Permettre la production collaborative de titres musicaux sous licence
libre, à partir de la mise en relation assurée par Yavsc.

### Formats supportés

- Audio (ex: WAV, FLAC, MP3)
- Partition (ex: MusicXML, LilyPond, PDF)
- MIDI

### Flux de production

1. Un client exprime un besoin musical
2. Des prestataires répondent avec des devis
3. Un **consensus est établi en amont** entre le client et les
   contributeurs sur la licence du livrable final
4. La collaboration produit les fichiers
5. Le titre est publié sous la licence choisie

---

## Licences

### Modèle `LicenceModele`

Géré par l'administration Yavsc. Chaque modèle porte :

- `EstLibre` (bool) — détermine le badge affiché sur le projet
- Les conditions : attribution, partage à l'identique, usage commercial,
  modification
- Une URL vers le texte officiel de la licence

### Seed initial

Licences Creative Commons préchargées : CC0, CC BY, CC BY-SA, CC BY-ND,
CC BY-NC, CC BY-NC-SA, CC BY-NC-ND (toutes en version 4.0) + ODbL 1.0.

### Badge projet

Rendu CSS/HTML — vert si `EstLibre`, orange sinon.

---

## Stack technique

- **Backend** : ASP.NET Core, C#
- **ORM** : Entity Framework Core (migrations générées)
- **Base de données** : PostgreSQL (provider Npgsql)
- **Frontend** : Razor views
- **Parsing frontmatter** : YamlDotNet

---

## Droits de Yavsc

### Administration

Le groupe des administrateurs prend la charge de :
- la Gestion des licences
- la Gestion des groupes d'utilisateurs (les modérateurs, en particulier)
- la Gestion des projets
- la Gestion des utilisateurs

### Gestion des utilisateurs et des groupes

En supposant que les certificats de letsencrypt sont au groupe `www-data`,
on peut créer l'utilisateur `yavsc` avec les droits suivants :

```bash
sudo addgroup yavsc --system
sudo adduser --ingroup yavsc --add-extra-groups www-data \
 --disabled-password --system yavsc
```

---

## Domaines d'activité

### Hiérarchie

Les activités sont organisées en arbre. Chaque activité peut avoir
une activité parente d'ordre plus général :

```
Droit                        ← racine, ancêtre commun
├── Musique
│   ├── Jazz
│   ├── Classique
│   └── ...
├── Graphisme
├── BTP
└── ...
```

Le domaine **Droit** est positionné à la racine — *nul n'est censé
ignorer la loi*. Sa position structurelle rend ses dictionnaires
naturellement disponibles dans tous les projets, sans attribut spécial.

### Modèle `DomaineActivite`

```
DomaineActivite
  - Id, Nom
  - ParentId (FK → DomaineActivite, nullable)
```

Pas de booléen `EstTransversal` — la transversalité est une conséquence
de la position dans l'arbre, pas un attribut explicite.

---

## Dictionnaires métier

### Principe

Les dictionnaires sont des **modèles publics** de termes métier,
maintenus par les modérateurs et alimentés par les fournisseurs.
Au moment de créer un projet, le dictionnaire est **copié** —
c'est un instantané, pas une référence vivante.

### Règle de résolution

> Un projet hérite des dictionnaires de son activité **et de tous
> ses ancêtres** jusqu'à la racine.

Ainsi, les termes juridiques sont toujours disponibles, quel que
soit le domaine du projet.

### Modèle

```
DictionnaireMetier
  - Id, Nom
  - DomaineActiviteId (FK)
  - Langue                      ← attribut du dictionnaire entier

TermeMetier
  - Id
  - DictionnaireMetierId (FK)
  - Mot
  - Definition
  - StatutValidation            ← Proposé | Validé | Rejeté
  - ProposeParId (FK → ApplicationUser)
  - ValidéParId  (FK → ApplicationUser, nullable)
```

### Cycle de vie d'un terme

1. Un **fournisseur** propose un terme dans son domaine et sa langue
2. Un **modérateur** valide → le terme intègre le dictionnaire public
3. À la création d'un projet, le dictionnaire est **associé par copie**

---

## Offre fournisseur et modèle canonique de demande

### Principe

Un fournisseur décrit son offre en **Markdown**, avec un en-tête
structuré délimité par `---` (frontmatter YAML standard) qui porte
les métadonnées de formulaire et de devis. Le corps est libre.

```markdown
---
formulaire: Prestation Musicale
devis: Arrangement Orchestral
---

Je propose des arrangements pour orchestre de chambre,
livraison sous 3 semaines, formats MusicXML et PDF...
```

### `ClasseFormulaire` et `ClasseDevis`

Ces deux référentiels sont **définis côté serveur par les modérateurs**,
pour servir de modèles de formulaires et de devis. Les fournisseurs
s'y conforment — ils ne peuvent pas en créer de nouveaux.

### Modèle canonique nom/valeur

```
ClasseFormulaire
  - Id, Nom
  - DomaineActiviteId (FK)
  └── ChampDemande
        - Nom          (ex: "tempo", "tonalité", "durée")
        - TypeValeur   (texte | entier | décimal | booléen | enum)
        - Obligatoire  (bool)
        - ValeurDefaut

ClasseDevis
  - Id, Nom
  - DomaineActiviteId (FK)

OffreFournisseur
  - Id
  - FournisseurId     (FK → ApplicationUser)
  - DomaineActiviteId (FK)
  - ContenuMarkdown          ← corps libre de l'offre
  - ClasseFormulaireId (FK)  ← extrait du frontmatter à la sauvegarde
  - ClasseDevisId      (FK)  ← extrait du frontmatter à la sauvegarde
```

### Demande client

À partir de l'offre, la demande instancie les champs du formulaire :

```
Demande
  - Id
  - OffreFournisseurId (FK)
  - ClientId (FK → ApplicationUser)
  └── ValeurChampDemande
        - ChampDemandeId (FK)
        - Valeur (string — sérialisé selon TypeValeur)
```

### Règle d'héritage

Les `ClasseFormulaire` et `ClasseDevis` disponibles pour une activité
incluent ceux de ses **ancêtres** dans l'arbre — cohérent avec la
règle de résolution des dictionnaires.

### Parsing du frontmatter avec YamlDotNet

```csharp
var parts = markdown.TrimStart()
                    .Substring(3)
                    .Split(new[] { "\n---" }, 2,
                           StringSplitOptions.None);
var yamlBlock = parts[0].Trim();
var corps     = parts[1].Trim();
var meta = deserializer.Deserialize<FrontmatterOffreResult>(yamlBlock);
```

---

## À documenter ensuite

- Modèle `ProjetMusical`
- Gestion des fichiers (audio, partition, MIDI)
- Workflow de validation de licence par l'administration
- Workflow de modération des termes métier
- Workflow complet de parsing et validation du frontmatter à la sauvegarde
