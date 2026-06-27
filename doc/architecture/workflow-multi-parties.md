# Workflow multi-parties

> **Récapitulatif** : Yavsc orchestre des projets multi-parties
> (client, fournisseur, coordinateur) avec mise en relation,
> sous-traitance et signature de licence en amont de la production.
> Détail dans cette page, racine de l'architecture : [Architecture.md](../Architecture.md).

## Rôle de chaque partie

| Rôle          | Type de compte     | Description                                               |
|---------------|--------------------|-----------------------------------------------------------|
| Client        | Pro ou particulier | Exprime le besoin, valide les devis, co-signe la licence  |
| Fournisseur   | Pro                | Répond aux besoins, peut sous-traiter                     |
| Coordinateur  | Pro ou particulier | Orchestre le projet sans relation de facturation directe  |

## Qui peut initier un projet

Le projet peut être initié par n'importe quelle partie :

- Un **client** (particulier ou pro) qui exprime un besoin
- Un **fournisseur** qui propose une offre ou monte un collectif
- Un **tiers coordinateur** qui orchestre sans être client ni prestataire

## Sous-traitance

Un fournisseur peut faire appel à d'autres fournisseurs,
**avec accord explicite du client**. Chaque sous-traitant :

- Est visible dans le projet
- Co-signe l'accord de licence
- Peut recevoir un devis distinct

## Distinction B2B / B2C

La distinction est portée par le **type de compte** :

- Compte **pro** : SIRET, TVA, facturation professionnelle
- Compte **particulier** : usage personnel, sans obligations fiscales pro

Un projet peut mélanger les deux (ex : un particulier client,
plusieurs prestataires pro) — Yavsc n'impose pas d'homogénéité.

## États d'un projet

```
Initié → En recherche de parties → Devis en cours
       → Accord de licence signé → En production
       → Livré → Publié (si licence libre)
```

## Contrainte clé

L'accord de licence est établi **avant** tout début de production,
signé (ou validé) par toutes les parties : client, fournisseurs,
et sous-traitants éventuels.

## Domaine musical — Titres collaboratifs

Permet la production collaborative de titres musicaux sous licence
libre, à partir de la mise en relation assurée par Yavsc.

### Formats supportés

- Audio (ex : WAV, FLAC, MP3)
- Partition (ex : MusicXML, LilyPond, PDF)
- MIDI

### Flux de production

1. Un client exprime un besoin musical
2. Des prestataires répondent avec des devis
3. Un **consensus est établi en amont** entre le client et les
   contributeurs sur la licence du livrable final
4. La collaboration produit les fichiers
5. Le titre est publié sous la licence choisie

## Voir aussi

- [Architecture.md](../Architecture.md) — racine.
