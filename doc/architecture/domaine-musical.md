# Domaine musical — Titres collaboratifs

> **Récapitulatif** : Yavsc permet de produire collaborativement
> des titres musicaux sous licence libre, en s'appuyant sur la
> mise en relation multi-parties. Détail dans cette page, racine
> de l'architecture : [Architecture.md](../Architecture.md).

## Objectif

Permettre la production collaborative de titres musicaux sous
licence libre, à partir de la mise en relation assurée par Yavsc.

## Formats supportés

- Audio (ex : WAV, FLAC, MP3)
- Partition (ex : MusicXML, LilyPond, PDF)
- MIDI

## Flux de production

1. Un client exprime un besoin musical
2. Des prestataires répondent avec des devis
3. Un **consensus est établi en amont** entre le client et les
   contributeurs sur la licence du livrable final
4. La collaboration produit les fichiers
5. Le titre est publié sous la licence choisie

## Contraintes spécifiques

- L'accord de licence sur le livrable musical est une instance
  du modèle `LicenceModele` détaillé dans
  [licences.md](licences.md).
- L'arborescence d'activités musicales (`Musique → Jazz`,
  `Musique → Classique`, etc.) est décrite dans
  [domaines-activite.md](domaines-activite.md).

## Voir aussi

- [Architecture.md](../Architecture.md) — racine.
- [workflow-multi-parties.md](workflow-multi-parties.md) — rôle
  client/fournisseur/coordinateur dans la production musicale.
- [licences.md](licences.md) — modèle de licence applicable au
  livrable.
