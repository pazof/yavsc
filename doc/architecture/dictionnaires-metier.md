# Dictionnaires métier

> **Récapitulatif** : Les dictionnaires sont des modèles publics
> de termes métier, validés par les modérateurs et **copiés**
> dans chaque projet (instantané, pas référence vivante).
> L'héritage suit l'arbre des activités. Détail dans cette page,
> racine de l'architecture : [Architecture.md](../Architecture.md).

## Principe

Les dictionnaires sont des **modèles publics** de termes métier,
maintenus par les modérateurs et alimentés par les fournisseurs.
Au moment de créer un projet, le dictionnaire est **copié** —
c'est un instantané, pas une référence vivante.

> Note historique : une première ébauche de cette page existait
> à `doc/Dictionnaire.md`. Elle a été absorbée ici.

## Règle de résolution

> Un projet hérite des dictionnaires de son activité **et de tous
> ses ancêtres** jusqu'à la racine.

Ainsi, les termes juridiques sont toujours disponibles, quel que
soit le domaine du projet.

Le modèle de dictionnaire du contrat est le dictionnaire défini
par l'activité ciblée par le client.

## Dictionnaires juridiques

Les dictionnaires juridiques sont des modèles publics de termes
juridiques, maintenus et alimentés par les modérateurs. Ils
figurent à la racine de l'arbre (sous `Droit`) et profitent donc
à toutes les activités par construction.

> Ce dictionnaire juridique est simplement fondamental, par
> construction, c'est la base de tous les dictionnaires.

## Modèle

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

## Cycle de vie d'un terme

1. Un **fournisseur** propose un terme dans son domaine et sa langue
2. Un **modérateur** valide → le terme intègre le dictionnaire public
3. À la création d'un projet, le dictionnaire est **associé par copie**

## Voir aussi

- [Architecture.md](../Architecture.md) — racine.
- [domaines-activite.md](domaines-activite.md) — l'arbre des
  activités qui justifie la règle d'héritage.
