# Matrice intentions -> agent -> preuves

Cette matrice aide a choisir rapidement l'agent adapte et a exiger
une sortie verifiable.

| Intention developpeur | Agent principal | Entrees minimales | Sortie minimale attendue | Verification |
|---|---|---|---|---|
| Comprendre un BC avant changement | Explore | BC cible, profondeur, contrainte de perimetre | Composants, points d'entree, tests relies, risques | Lire les fichiers cites + confirmer tests proposes |
| Decomposer une tache transverse | Plan | Objectif, contraintes, definition of done | Etapes ordonnees, dependances, criteres de verif | Verifier que chaque etape a une preuve observable |
| Implementer une modif locale | Copilot | Fichier cible, comportement attendu, conventions | Patch minimal, justification courte | Build/test du projet impacte |
| Ajouter un test smoke | Copilot (+Explore) | Route/endpoint, projet de test cible | Test + commande cible | Execution test cible |
| Corriger une regression | Plan + Copilot | Symptome, zone suspecte, test attendu | Fix + test NonRegression | Test rouge avant, vert apres |
| Diagnostiquer flux PostIt/OIDC | Explore + Plan | Flux, symptome, plateforme | Carte du flux + hypotheses testables | Verification manuelle + tests existants |

## Regles d'arbitrage

- Si l'intention est "comprendre": commencer par Explore.
- Si l'intention est "orchestrer": commencer par Plan.
- Si l'intention est "produire": utiliser Copilot apres cadrage.
- Si une sortie n'inclut pas de preuve, elle est incomplete.
