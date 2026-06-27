

# Yavsc

C'est une application mettant en oeuvre une prise de contact entre un demandeur de services et son éventuel prestataire associé.

# Statut actuel des actions GitHub

* [![Build and Push Yavsc Apk](https://github.com/pazof/yavsc/actions/workflows/docker-publish-android.yml/badge.svg)](https://github.com/pazof/yavsc/actions/workflows/docker-publish-android.yml)

* [![Build and Push Yavsc Production Image](https://github.com/pazof/yavsc/actions/workflows/docker-publish-backend.yml/badge.svg)](https://github.com/pazof/yavsc/actions/workflows/docker-publish-backend.yml)


# Construction et déploiement

## Construction

```bash
 dotnet build
```

et, pour execution en environment de développement

```bash
~/workspace/yavsc/Yavsc @ ASPNETCORE_ENV=Development dotnet run
```

## Tests

Utilisez GNU/Makefile (et visitez le code, dans le dossier `test` ):

[TODO] Depuis le répertoire racine:

```bash
make test
```

## Installation / Déploiement / Développement

### les services et l'API

### La Prod

`cd srv/Yavsc` : `make pushInProd CONFIGURATION=Release`.

puis, pour une première installation
`make install_service`.

## Fonctionnalités (encore en cours de développement)

Elle est censée aboutir à une prise commande,
un payement du client, à une collecte du retour du client, et à un paiement du prestataire de services.

Elle comprendra une gestion des litiges.

Elle expose une messagerie instantanée, disponible depuis un navigateur Web ou depuis l’application mobile,
pouvant garantir la preservation du secret sur toute information personnelle,
du client comme du prestataire.

Ni le client ni le prestataire ne sont anonymes pour l'application,
ils sont même formellement authentifiés, au moment de leur accord pour une première
facturation en ligne, à l'occasion:

* pour le client, à la validation d'une commande facturée (de prestation à un prestataire, ou autre).
* pour le prestataire, de la validation de son profile professionnel, qui implique l’acquittement de son adhésion forfaitaire.

La séquence logique (et simplifiable) d'une prestation canonique (sans annulation ni reclamation) est la suivante :

1. Une commande intervient auprès d'un prestataire, elle est chiffrée et le paiement est provisionné par PayPal, non collecté.
2. Notifié, le prestataire valide un devis, avec arrhes ou avance. il signe son devis, qui peu contenir des documents attachés à faire signer par le client, un ou des contrats, stokés au format Markdown par le prestataire dans ses contrats à faire signer.
3. à son tour, le client est notifié et signe le devis aussi
4. Les arrhes ou avances sont débitées sur le champ
5. 10 jours avant la date de la prestation le reste du paiement est collecté

Dans le cas des arrhes, à tout moment, jusqu'avant la date et l'heure de la prestation, le client ou le prestataire peuvent annuler:

* Le prestataire peut le faire, en rendant les arrhes majorées de 20%
* Le client peut le faire, en perdant les arrhes.
* Le prestataire peut déléguer à une équipe de son choix un filtrage des demandes des clients.

## Limitations

* à une commande, une prestation
* Dans le cas de l'avance, une fois le paiement client autorisé, pour le moment, aucune annulation de la prestation n'est supportée.
* Une fois passée la date de la prestation, toute reclamation nécessitera l'intervention d'un système auxiliaire (un processus humain?)
* Un seul moyen de paiement: PayPal, depuis le Web ou l'application mobile, son interface dite dépréciée NVP/SOAP.
* Elle ne prendra pas en charge, du moins pas encore, ni la saisie de structures de projets complexes, ni ticketing associé à la prestation.
* Les professionnels sont tous considérés comme tierces parties, hormis le propriétaire de l'installation, dont les identifiants PayPal sont utilisés pour collecter tous les paiements. TODO Aucune edition de fiche de paye ni paiement en masse ne sont supportés pour l'instant. Seul les payments unitaires sus-cités le sont.

## Paramétrage

### Fichiers de configuration d'Yavsc.Org

Le paramétrage runtime d'Yavsc.Org se fait via `appsettings-org.json`,
lu par ASP.NET Core selon la convention standard (`appsettings.<ASPNETCORE_ENVIRONMENT>.json`
est fusionné par-dessus). Trois fichiers coexistent dans
`src/Yavsc.Org/`, avec trois rôles distincts :

| Fichier                                      | Rôle                                                                                             | Commité ? |
|----------------------------------------------|--------------------------------------------------------------------------------------------------|-----------|
| `appsettings-org.json`                       | **Modèle**. Valeurs anonymisées (`[Your domaine name]`, `[YOURSMTPHOST]`, …). Sert de référence.| Oui       |
| `appsettings-org.Development.json`           | Surcharge locale pour `ASPNETCORE_ENVIRONMENT=Development`. Concrètement, `Site.Authority` et `Site.ExternalUrl` pointent sur `https://localhost:5001`, `Site.Title`/`Slogan` sont adaptés, et la `ConnectionStrings.YavscConnection` est fournie. **Ignoré par `.gitignore`** (`appsettings-*.*.json`) : chaque développeur crée le sien à partir de ce qui est documenté ici et du modèle `appsettings-org.json`. | Non       |
| `appsettings-org-dist.json`                  | Généré par la cible `copy-binaries` du `contrib/Makefile` (alias `make reinstall`) à partir d'`appsettings-org.json` lors du déploiement, puis copié dans `$(BASEAPPDIR)`. C'est un clone du modèle, juste renommé pour signaler « à éditer ». | Non       |

**Première installation sur un serveur :**

```bash
make install_service        # construit, déploie, active systemd
cd $BASEAPPDIR              # voir contrib/.env
ls appsettings-org*.json    # -> appsettings-org-dist.json (le modèle, jamais éditer celui-là)
cp appsettings-org-dist.json appsettings-org.json
$EDITOR appsettings-org.json
```

Le `Makefile` renomme `appsettings-org.json` en `appsettings-org-dist.json`
juste avant le `cp -a` vers `$(BASEAPPDIR)`, et **supprime** la version
`appsettings-org.json` du publish dir. Conséquence : un `make reinstall`
ultérieur n'écrasera jamais ton `appsettings-org.json` édité — il
recopiera un `appsettings-org-dist.json` neuf par-dessus, sans
supprimer ton édition. Si tu veux repartir d'un modèle vierge, supprime
d'abord `appsettings-org.json` du serveur ; sinon, laisse-le en place.

**Valeurs minimales à renseigner pour un serveur de production :**

- `Site.Authority` — issuer OIDC visible de l'extérieur (URL exacte sous
  laquelle `/.well-known/openid-configuration` est servi, par exemple
  `https://yavsc.pschneider.fr`). C'est la valeur que PostIt et toutes
  les autres applications clientes passent dans `Authority`.
- `Site.ExternalUrl` — URL canonique du serveur, utilisée pour générer
  les liens absolus dans les e-mails, construire les RedirectUris des
  clients OIDC tiers, etc. **Depuis la migration PKCE de PostIt, le
  seed EF Core d'IdentityServer utilise `Site.ExternalUrl` pour
  autoriser une RedirectUri du client `postit`** : cela permet à PostIt
  d'être lancé depuis une page web de Yavsc.Org (iframe launcher)
  sans rejet `redirect_uri mismatch` de l'OP.
- `ConnectionStrings.YavscConnection` — chaîne de connexion PostgreSQL
  (utilisateur, mot de passe, hôte, base). Privilégier
  `dotnet user-secrets` ou des variables d'environnement `ASPNETCORE_*`
  plutôt qu'un mot de passe en clair dans le fichier.
- `Smtp.*` — hôte, port, identifiants SMTP pour l'envoi d'e-mails
  transactionnels.
- `Authentication.PayPal.*` et `Authentication.Google.*` — clés d'API
  pour les fournisseurs d'identité externes utilisés dans les flows
  OAuth/OIDC.

### Administration

Une fois le service disponible, s'enregistrer, et
Visiter l'url `/Administration/Take`

## Une nouvelle activité

On gère les activité en faisant partie du groupe des commerciaux (`FrontOffice`),
on crée des activités en y associant des formulaires de commande et une
classe de paramétrage de profiles professionnels.

# Développement

## Un nouvel environnement d'execution

L'impact de l'usage d'un nouveau nom d’environnement d'execution, à l'heure de cet écrit, ressemble à ceci:

* Ajustement des listes d’environnements cités dans les pages:
  * ~/Views/Shared/_Layout.cshtml
  * ~/Views/Shared/_ValidationScriptsPartial.cshtml
  * ~/Views/Home/Index.cshtml
  * ~/Views/Home/About.cshtml

# Remerciements

Photo de <a href="https://unsplash.com/fr/@dewang?utm_source=unsplash&utm_medium=referral&utm_content=creditCopyText">Dewang Gupta</a>sur <a href="https://unsplash.com/fr/photos/homme-debout-pres-dun-arbre-Mu3T3DmvQQw?utm_source=unsplash&utm_medium=referral&utm_content=creditCopyText">Unsplash</a>
