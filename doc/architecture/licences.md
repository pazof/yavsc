# Licences

> **Récapitulatif** : Yavsc référence des modèles de licence
> (Creative Commons + ODbL) maintenus par l'administration. Chaque
> projet signe l'accord avant toute production. Détail dans cette
> page, racine de l'architecture : [Architecture.md](../Architecture.md).

## Modèle `LicenceModele`

Géré par l'administration Yavsc. Chaque modèle porte :

- `EstLibre` (bool) — détermine le badge affiché sur le projet
- Les conditions : attribution, partage à l'identique, usage
  commercial, modification
- Une URL vers le texte officiel de la licence

## Seed initial

Licences Creative Commons préchargées : CC0, CC BY, CC BY-SA,
CC BY-ND, CC BY-NC, CC BY-NC-SA, CC BY-NC-ND (toutes en version
4.0) + ODbL 1.0.

## Badge projet

Rendu CSS/HTML — vert si `EstLibre`, orange sinon.

## Cycle de vie

1. L'administration crée ou met à jour un `LicenceModele`.
2. À la création d'un projet, le client choisit un modèle
   dans la liste des licences applicables à son activité.
3. Le projet passe par l'état **Devis en cours** avant que
   l'accord ne soit signé par toutes les parties (cf.
   [workflow-multi-parties.md](workflow-multi-parties.md)).
4. Une fois signé, l'état passe à **En production** ; le badge
   du projet reflète `EstLibre`.

## Voir aussi

- [Architecture.md](../Architecture.md) — racine.
- [workflow-multi-parties.md](workflow-multi-parties.md) —
  contrainte clé : accord signé avant production.
