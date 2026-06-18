Demande
  - Id
  - OffreFournisseurId (FK)
  - ClientId (FK → ApplicationUser)
  └── ValeurChampDemande
        - ChampDemandeId (FK)
        - Valeur (string — sérialisé selon TypeValeur)
