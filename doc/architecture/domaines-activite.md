# Domaines d'activité

> **Récapitulatif** : Yavsc organise ses activités en arbre, avec
> `Droit` à la racine pour rendre les termes juridiques
> disponibles partout. Pas de booléen "transversal" : la
> transversalité découle de la position dans l'arbre. Détail dans
> cette page, racine de l'architecture : [Architecture.md](../Architecture.md).

## Hiérarchie

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

Le domaine **Droit** est positionné à la racine — *nul n'est
censé ignorer la loi*. Sa position structurelle rend ses
dictionnaires naturellement disponibles dans tous les projets,
sans attribut spécial.

## Modèle `DomaineActivite`

```
DomaineActivite
  - Id, Nom
  - ParentId (FK → DomaineActivite, nullable)
```

Pas de booléen `EstTransversal` — la transversalité est une
conséquence de la position dans l'arbre, pas un attribut
explicite.

## Conséquences sur les autres référentiels

Cette arborescence irrigue :

- les **dictionnaires métier** (cf. [dictionnaires-metier.md](dictionnaires-metier.md)) :
  un projet hérite des dictionnaires de ses ancêtres.
- les **classes de formulaire et de devis** (cf.
  [offres-frontmatter.md](offres-frontmatter.md)) : un fournisseur
  rattaché à `Musique → Jazz` a accès aux modèles de son
  activité et de ses ancêtres.

## Voir aussi

- [Architecture.md](../Architecture.md) — racine.
- [dictionnaires-metier.md](dictionnaires-metier.md) — règle
  d'héritage en arbre.
- [offres-frontmatter.md](offres-frontmatter.md) — modèle
  canonique nom/valeur pour formulaires et devis.
