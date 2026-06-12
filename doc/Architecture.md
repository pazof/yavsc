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

---

## À documenter ensuite

- Modèle `ProjetMusical`
- Gestion des fichiers (audio, partition, MIDI)
- Workflow de validation de licence par l'administration

