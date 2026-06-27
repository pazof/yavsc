# Architecture de Yavsc

## Vision générale

Yavsc est une plateforme de mise en relation client/fournisseur,
spécialisable par domaine d'activité, avec une origine musicale.

Elle gère des **devis** (pas de facturation directe), et s'intègre
dans des workflows collaboratifs pouvant aboutir à des livrables
sous licence libre.

## Pages de détail

Chaque sujet est détaillé dans sa propre page (sous
`doc/architecture/`) ; cette racine tient lieu de sommaire.

| Sujet                                          | Page                                                              |
|------------------------------------------------|-------------------------------------------------------------------|
| Workflow multi-parties (client / fournisseur / coordinateur, sous-traitance, états) | [workflow-multi-parties.md](architecture/workflow-multi-parties.md) |
| Domaine musical (titres collaboratifs)         | [domaine-musical.md](architecture/domaine-musical.md)             |
| Licences (`LicenceModele`, seed CC/ODbL, badge)| [licences.md](architecture/licences.md)                           |
| Domaines d'activité (arbre, `DomaineActivite`) | [domaines-activite.md](architecture/domaines-activite.md)         |
| Dictionnaires métier (héritage en arbre)       | [dictionnaires-metier.md](architecture/dictionnaires-metier.md)   |
| Offre fournisseur + frontmatter (`ClasseFormulaire`, `ClasseDevis`, parsing) | [offres-frontmatter.md](architecture/offres-frontmatter.md) |
| PostIt / OIDC (client desktop, custom URI scheme, silent refresh) | [postit-oidc.md](architecture/postit-oidc.md)        |

## Stack technique

- **Backend** : ASP.NET Core, C#
- **ORM** : Entity Framework Core (migrations générées)
- **Base de données** : PostgreSQL (provider Npgsql)
- **Frontend** : Razor views
- **Parsing frontmatter** : YamlDotNet
- **Client desktop** : Avalonia 12 (PostIt — détails dans
  [postit-oidc.md](architecture/postit-oidc.md))

## Droits de Yavsc

### Administration

Le groupe des administrateurs prend la charge de :

- la Gestion des licences
- la Gestion des groupes d'utilisateurs (les modérateurs, en particulier)
- la Gestion des projets
- la Gestion des utilisateurs

### Gestion des utilisateurs et des groupes

En supposant que les certificats de letsencrypt sont au groupe
`www-data`, on peut créer l'utilisateur `yavsc` avec les droits
suivants :

```bash
sudo addgroup yavsc --system
sudo adduser --ingroup yavsc --add-extra-groups www-data \
 --disabled-password --system yavsc
```

## À documenter ensuite

- Modèle `ProjetMusical`
- Gestion des fichiers (audio, partition, MIDI)
- Workflow de validation de licence par l'administration
- Workflow de modération des termes métier
- Workflow complet de parsing et validation du frontmatter à la sauvegarde
