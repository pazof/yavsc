
ClasseDevis
  - Id, Nom
  - DomaineActiviteId (FK)

OffreFournisseur
  - Id
  - FournisseurId (FK → ApplicationUser)
  - DomaineActiviteId (FK)
  - ContenuMarkdown          ← corps libre de l'offre
  - ClasseFormulaireId (FK)  ← extrait du frontmatter à la sauvegarde
  - ClasseDevisId (FK)       ← extrait du frontmatter à la sauvegarde
  
  ClasseFormulaire
  - Id, Nom
  - DomaineActiviteId (FK)
  └── ChampDemande
        - Nom          (ex: "tempo", "tonalité", "durée")
        - TypeValeur   (texte | entier | décimal | booléen | enum)
        - Obligatoire  (bool)
        - ValeurDefaut

ClasseDevis
  - Id, Nom
  - DomaineActiviteId (FK)

OffreFournisseur
  - Id
  - FournisseurId (FK → ApplicationUser)
  - DomaineActiviteId (FK)
  - ContenuMarkdown          ← corps libre de l'offre
  - ClasseFormulaireId (FK)  ← extrait du frontmatter à la sauvegarde
  - ClasseDevisId (FK)       ← extrait du frontmatter à la sauvegarde
