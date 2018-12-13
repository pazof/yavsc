
[![Build Status](https://travis-ci.org/pazof/yavsc.svg?branch=vnext)](https://travis-ci.org/pazof/yavsc)

# Yavsc

C'est une application mettant en oeuvre une prise de contact entre un demandeur de services et son éventuel préstataire associé.

# Construction et déploimenent

## Construction

Le code est du c sharp, dont les librairies sont restorées et le tout compilé avec les librairies DNX de M$, téléchargeable en executant le script d'installation suivant (c.f. `/.travis.yml`):

    curl --insecure -sSL https://lua.pschneider.fr/files/Paul/dnx-install.sh | bash && DNX_USER_HOME=`pwd -P`/dnx . ./dnx/dnvm/dnvm.sh && cd Yavsc && dnu restore

Une fois l'environnement ainsi pollué, executer, depuis le sous dossier `Yavsc`:

    dnu build

L'execution, avec un runtime Mono, echoura dans ses version récentes.
Celui ci convient:

    [monoperso] ~/workspace/yavsc/Yavsc @ mono --version
    Mono JIT compiler version 4.6.2 (Stable 4.6.2.7/08fd525 jeudi 18 janvier 2018, 13:10:54 (UTC+0100))
        TLS:           __thread
        SIGSEGV:       altstack
        Notifications: epoll
        Architecture:  amd64
        Disabled:      none
        Misc:          softdebug 
        LLVM:          supported, not enabled.
        GC:            sgen


et, pour execution en environement de développement

    [monoperso] ~/workspace/yavsc/Yavsc @ ASPNET_ENV=Development dnx web
    warn: Yavsc.Startup[0]
      AppData was not found in environment variables
    warn: Yavsc.Startup[0]
      It has been set to : /home/paul/workspace/yavsc/Yavsc/AppDataDevelopment
    Hosting environment: Development
    Now listening on: http://*:5000
    Application started. Press Ctrl+C to shut down.

Si vous êtes arrivé jusqu'ici, vous devriez pouvoir visiter la home page 
 [ici](http://localhost:5000).

## Tests

Utilisez GNU/Makefile (et visitez le code, dans le dossier `test` ):


Depuis le répertoire racine:

``` 
make test
```


## Installation / Déploiment / Développement

### les services kestrel et kestrel-pre

[TODO]

### la configuration Apache 2

[TODO]

### la mise en pré-production

Pour déployer le tout en production, on peut d'abord déployer en "pré-production",
Afin de pouvoir tester manuellement quelque dernier développement :


```
cd Yavsc
make pushInPre # arrete kestrel-pre, pousse tout dans DESTDIR=/srv/www/yavscpre avec rsync,
               # et redemarre kestrel-pre 
```

Une fois sûr de vous, et une fois que Git dit propre votre copie de travail, depuis le répertoire `Yavsc`, lancez `make pushInProd`.

## Fonctionalités

Elle est censée aboutir à une prise commande,
un payement du client, à une collecte du retour du client, et à un paiment du prestataire de services.

Elle comprendra une gestion des litiges.

Elle expose une messagerie instantanée, disponible depuis un navigateur Web ou depuis l'appplication mobile,
pouvant garantir la preservation du secret sur toute information personnelle,
du client comme du prestataire.

Ni le client ni le prestataire ne sont anonymes pour l'application,
il sont même formellement authentifies, au moment de leur accord pour une première
facturation en ligne, à l'occasion:

* pour le client, à la validation d'une commande facturée (de prestation à un prestataire, ou autre).
* pour le prestataire, de la validation de son profile proféssionnel, qui implique l'acquitement de son adhésion forfaitaire.

La séquence logique (et simplifiable) d'une prestation canonique (sans annulation ni reclamation) est la suivante :

1. Une commande intervient auprés d'un prestataire, elle est chiffrée et le paiment est provisioné par PayPal, non collécté.
2. Notifié, le prestataire valide un devis, avec arrhes ou avance. il signe son devis, qui peu contenir des documents attachés à faire signer par le client, un ou des contrats, stokés au format Markdown par le prestataire dans ses contrats à faire signer.
3. à son tour, le client est notifié et signe le devis aussi
4. Les arrhes ou avances sont débitées sur le champ
5. 10 jours avant la date de la prestation le reste du paiement est collecté

Dans le cas des arrhes, à tout moment, jusqu'avant la date et l'heure de la prestation, le client ou le prestataire peuvent annuler:

* Le prestataire peut le faire, en rendant les arrhes majorées de 20%
* Le client peut le faire, en perdant les arrhes.
* Le prestataire peut déléguer à une équipe de son choix un filtrage des demandes des clients.

## Limitations temporaires

* à une commande, une prestation, un paiment

## Limitations conceptuelles

* Dans le cas de l'avance, une fois le paiment client autorisé, pour le moment, aucune annulation de la préstation n'est supportée.
* Une fois passée la date de la prestation, toute reclamation nécessitera l'intervention d'un système auxiliaire (un processus humain?)
* Un seul moyen de paiment: PayPal, depuis le Web ou l'application mobile, son interface dite dépréciée NVP/SOAP.
* Elle ne prendra pas en charge, du moins pas encore, ni la saisie de structures de projets complexes, ni ticketing associé à la prestation.
* Les professionnels sont tous considérés comme tierces parties, horsmis le propriétaire de l'installation, dont les identifiants PayPal sont utilisés pour collecter tous les paiments. TODO Aucune edition de fiche de paye ni paiment en masse ne sont supportés pour l'instant. Seul les payments unitaires sus-cités le sont.

## Développement

## Une nouvelle activité

L'impact d'une custo de son activité pourrait à peu près tout concerner.
Un bon point de départ est la création d'un controller de commande dédié, enrichi des données de profile associées à un nouveau type de profiles prestataire.

Ceci implique:

* Un modèle de donnée, un controleur web, ses vues et son API pour:
  * Le profile prestataire, dont la donnée est représentée par une classe arbitraire
  * L'éventuelle commande customisée, dont la donnée réalise l'objet abstrait 'NominativeServiceCommand'

## Un nouvel environnement d'execution

L'impact de l'usage d'un nouveau nom d'environement d'execution, à l'heure de cet écrit, ressemble à ceci:

* Ajustement des listes d'environements cités dans les pages:
  * ~/Views/Shared/_Layout.cshtml
  * ~/Views/Shared/_ValidationScriptsPartial.cshtml
  * ~/Views/Home/Index.cshtml
  * ~/Views/Home/About.cshtml

... et beaucoup plus si affinité!
