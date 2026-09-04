# Onboarding guide: agents IA + architecture + tests

Ce guide est optimise pour accelerer la prise en main des agents IA
(Copilot, Plan, Explore) dans Yavsc, avec une verification rapide
par les tests.

## Resultat attendu

A la fin du parcours, un contributeur doit pouvoir:
- Identifier les projets impactes par une modification.
- Choisir l'agent adapte a l'intention de travail.
- Produire une proposition de changement verifiable par les tests.

## Parcours en 3 modules

## Module A - Comprendre le terrain (30-45 min)

Objectif: acquerir une lecture fiable de l'architecture.

1. Lire [README.md](../README.md) puis [Architecture.md](Architecture.md).
2. Lire [architecture/decoupage-organisation.md](architecture/decoupage-organisation.md).
3. Selon le domaine:
   - Backend/API: [architecture/workflow-multi-parties.md](architecture/workflow-multi-parties.md)
   - PostIt: [architecture/postit.md](architecture/postit.md) puis [architecture/postit-oidc.md](architecture/postit-oidc.md)

Definition of done:
- Expliquer en 5 phrases quelles couches sont touchees.
- Citer le ou les points d'entree applicatifs a verifier.

## Module B - Boucle tests rapide (20-30 min)

Objectif: verifier rapidement sans lancer toute la suite.

1. Lire [testing.md](testing.md).
2. Lancer les smoke tests d'abord, puis mandatory selon le projet.
3. N'elargir au test complet que si le scope depasse le BC touche.

Definition of done:
- Fournir la commande test executee.
- Expliquer pourquoi ce niveau de test est suffisant.

## Module C - Usage agentique en production (30-40 min)

Objectif: utiliser les agents comme accelerateurs, pas comme boites noires.

1. Plan: decomposer la tache en etapes verifiables.
2. Explore: collecter le contexte code/doc precise.
3. Copilot: implementer localement et verifier.

Regles:
- Toujours donner un contexte explicite (fichier, but, contrainte).
- Demander des preuves observables (fichiers modifies, tests, risques).
- Refuser toute sortie non verifiable.

Definition of done:
- Une tache simple est livree avec:
  - Plan
  - Changement local
  - Preuve par test

## Routine continue (sans echeance fixe)

Rituels recommandes:
- Hebdo: revue des prompts qui ont bien fonctionne.
- Mensuel: mise a jour du present guide et du playbook.
- A chaque incident: ajouter un anti-pattern dans le playbook.

## Check-list de validation

- Le changement indique son impact architecture.
- Le choix de l'agent est justifie.
- La preuve test est incluse.
- Les risques residuels sont explicitement listes.
