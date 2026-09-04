# Playbook d'usage des agents IA (Yavsc)

Ce playbook normalise l'usage de Copilot, Plan et Explore dans le depot.
Il privilegie des sorties verifiables: fichiers, commandes tests, risques.

## Quand utiliser quel agent

- Plan: quand la tache est ambigue, transverse ou risquee.
- Explore: quand il faut cartographier rapidement des zones du code.
- Copilot: quand les specifications sont claires et localisees.

## Prompt type (base)

Utiliser ce squelette avant toute tache non triviale:

```text
Contexte: <projet/fichier/fonction>
Objectif: <resultat observable>
Contraintes: <style, archi, perimetre>
Verification: <tests exacts a lancer>
Sortie attendue: <fichiers modifies + risques>
```

## 4 scenarios de reference

## 1) Explorer un bounded context

Intention:
- Comprendre ou implementer un changement dans un BC sans regression laterale.

Prompt minimal:
```text
Explore le BC <nom> avec profondeur medium.
Retour: composants touches, points d'entree, tests existants et risques.
```

Preuves attendues:
- Carte des fichiers a modifier.
- Test(s) smoke/mandatory proposes.

## 2) Ajouter un smoke test

Intention:
- Couvrir rapidement un endpoint ou une route publique.

Prompt minimal:
```text
Propose un smoke test pour <route/endpoint> dans le projet de test approprie.
Respecte les conventions de doc/testing.md.
```

Preuves attendues:
- Fichier test cree/modifie.
- Commande precise pour executer le test cible.

## 3) Corriger une regression backend API

Intention:
- Corriger un bug sans casser un flux voisin.

Prompt minimal:
```text
Planifie puis implemente un fix de <symptome> dans <projet>.
Ajoute/ajuste un test NonRegression rouge puis vert.
```

Preuves attendues:
- Explication cause racine.
- Test non-regression associe.
- Commande d'execution et resultat attendu.

## 4) Tracer un flux PostIt/OIDC

Intention:
- Localiser une cassure d'authentification entre client et serveur.

Prompt minimal:
```text
Cartographie le flux OIDC PostIt: entrypoints, callback, stockage token,
refresh. Donne points de rupture probables et tests/verification proposes.
```

Preuves attendues:
- Liste ordonnee des etapes du flux.
- Fichiers critiques.
- Hypotheses testables.

## Anti-patterns a eviter

- Prompt sans objectif verifiable.
- Demande trop large sans perimetre de fichiers.
- Validation basee uniquement sur "ca semble correct".
- Pas de lien entre changement et niveau de test.

## Gate PR minimale (agent-assiste)

Avant validation:
- Impact architecture explicite.
- Rationale de choix agent explicite.
- Test(s) executes et justifies.
- Risques residuels documentes.
