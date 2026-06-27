# Offre fournisseur, formulaire, devis

> **Récapitulatif** : Le fournisseur rédige son offre en Markdown
> avec un en-tête frontmatter YAML qui pointe vers un
> `ClasseFormulaire` et un `ClasseDevis` définis par les
> modérateurs. La demande du client instancie les champs du
> formulaire. L'héritage suit l'arbre des activités. Détail dans
> cette page, racine de l'architecture : [Architecture.md](../Architecture.md).

> Note historique : des fragments existaient à `doc/Formulaires-devis.md`
> et `doc/Demande.md`. Ils sont absorbés ici.

## Principe

Un fournisseur décrit son offre en **Markdown**, avec un en-tête
structuré délimité par `---` (frontmatter YAML standard) qui porte
les métadonnées de formulaire et de devis. Le corps est libre.

```markdown
---
formulaire: Prestation Musicale
devis: Arrangement Orchestral
---

Je propose des arrangements pour orchestre de chambre,
livraison sous 3 semaines, formats MusicXML et PDF...
```

## `ClasseFormulaire` et `ClasseDevis`

Ces deux référentiels sont **définis côté serveur par les
modérateurs**, pour servir de modèles de formulaires et de devis.
Les fournisseurs s'y conforment — ils ne peuvent pas en créer
de nouveaux.

## Modèle canonique nom/valeur

```
ClasseFormulaire
  - Id, Nom
  - DomaineActiviteId (FK)
  └── ChampDemande
        - Nom          (ex : "tempo", "tonalité", "durée")
        - TypeValeur   (texte | entier | décimal | booléen | enum)
        - Obligatoire  (bool)
        - ValeurDefaut

ClasseDevis
  - Id, Nom
  - DomaineActiviteId (FK)

OffreFournisseur
  - Id
  - FournisseurId     (FK → ApplicationUser)
  - DomaineActiviteId (FK)
  - ContenuMarkdown          ← corps libre de l'offre
  - ClasseFormulaireId (FK)  ← extrait du frontmatter à la sauvegarde
  - ClasseDevisId      (FK)  ← extrait du frontmatter à la sauvegarde
```

## Demande client

À partir de l'offre, la demande instancie les champs du formulaire :

```
Demande
  - Id
  - OffreFournisseurId (FK)
  - ClientId (FK → ApplicationUser)
  └── ValeurChampDemande
        - ChampDemandeId (FK)
        - Valeur (string — sérialisé selon TypeValeur)
```

## Règle d'héritage

Les `ClasseFormulaire` et `ClasseDevis` disponibles pour une
activité incluent ceux de ses **ancêtres** dans l'arbre —
cohérent avec la règle de résolution des dictionnaires
([dictionnaires-metier.md](dictionnaires-metier.md)).

## Parsing du frontmatter avec YamlDotNet

Le parsing du frontmatter vit dans
`src/Yavsc.Server/Services/FrontmatterParser.cs` (introduit dans
le commit `4034c399 Front matters`). Squelette :

```csharp
var parts = markdown.TrimStart()
                    .Substring(3)
                    .Split(new[] { "\n---" }, 2,
                           StringSplitOptions.None);
var yamlBlock = parts[0].Trim();
var corps     = parts[1].Trim();
var meta = deserializer.Deserialize<FrontmatterOffreResult>(yamlBlock);
```

Le résultat `FrontmatterOffreResult` est sérialisé en JSON pour
alimenter `OffreFournisseur.ClasseFormulaireId` /
`ClasseDevisId` à la sauvegarde.

## Voir aussi

- [Architecture.md](../Architecture.md) — racine.
- [domaines-activite.md](domaines-activite.md) — arbre des
  activités et héritage.
