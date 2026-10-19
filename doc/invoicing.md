# Facturation et paiement après devis signé

## Objectif

Ce document fixe le plan métier et technique pour le flux qui suit la validation du devis :

- le devis est signé par les deux parties,
- le prestataire déclenche la création d'une facture non acquittée,
- la facture contient la ligne associée au mode de paiement choisi par le prestataire,
- le client règle le montant partiel ou total,
- la facture reste non acquittée tant que le total n'est pas intégralement payé.

## Règle métier de base

1. Le devis n'est considéré comme valide que lorsqu'il est signé par le fournisseur et par le client.
2. Une facture ne peut être générée qu'après cette double validation.
3. Le type de contrat choisi dans les settings du prestataire détermine le mode de règlement :
   - arrhes,
   - avance,
   - ou autre mode de facturation supporté par le système.
4. Le montant payé partiellement est enregistré comme paiement partiel, sans transformer la facture en facture entièrement payée.
5. La facture est marquée comme acquittée uniquement lorsque le montant total est effectivement réglé.
6. L'addition / le relevé de paiement doit mentionner le paiement effectué, sans prétendre que la facture est déjà entièrement acquittée.

## Flux attendu

### 1. Devis signé par les deux parties

Le flux contractuel suit la séquence :

- commande / demande,
- devis préparé,
- validation prestataire,
- validation client,
- double signature persistée.

Une fois cette étape passée, le devis est un contrat exploitable pour la facture.

### 2. Génération de la facture non acquittée

Le prestataire déclenche la création de la facture depuis le contexte du devis validé.

Le système doit alors :

- identifier le devis validé,
- récupérer le client et le prestataire,
- sélectionner le mode de contrat défini dans les settings du prestataire,
- ajouter la ligne de frais correspondante à la facture,
- créer la facture dans un état "non acquittée".

### 3. Paiement partiel

Le client peut payer en premier :

- l'avance,
- les arrhes,
- ou une tranche partielle.

Dans ce cas :

- le paiement est enregistré,
- le montant payé est ajouté au total enregistré,
- la facture reste "non acquittée" tant qu'elle n'a pas été réglée intégralement,
- l'addition doit refléter la transaction effectuée sans annoncer un paiement total abusif.

### 4. Acquittement total

Lorsque le client a payé la somme totale due :

- le statut de la facture passe à "payée" / "acquittée",
- la facture peut être considérée comme totalement soldée,
- la génération de documents ou notifications finales peut s'appuyer sur cet état.

## Règles de codage à respecter

### Règle 1 — ne pas confondre paiement partiel et paiement total

Le modèle de paiement ne doit pas se limiter à un booléen "payé / non payé".

Le bon niveau de modélisation est :

- montant dû,
- montant payé,
- montant restant,
- statut de paiement partiel / total,
- état d'acquittement final.

### Règle 2 — la facture ne doit pas être "payée" trop tôt

Le système ne doit pas marquer la facture comme entièrement payée à la première transaction.

L'état métier doit être calculé de manière explicite :

- `montantPaye < montantTotal` => facture non acquittée,
- `montantPaye >= montantTotal` => facture acquittée (reste à payer borné à zéro).

### Règle 3 — les arrhes / avances doivent rester cohérents avec le contexte contractuel

Le mode choisi par le prestataire (arrhes ou avance) doit être représenté de façon explicite dans le flux de génération de facture et dans l'addition.

Le comportement attendu est :

- la ligne de frais correspondante est ajoutée à la facture,
- le paiement initial est comptabilisé correctement,
- le solde restant reste visible et exploitable.

## Plan d'implémentation proposé

### Étape 1 — modéliser les règles de paiement

Implémentée sur le point d'entrée existant `IBillable` /
`NominativeServiceCommand`, sans créer encore d'entité Facture distincte :

- `AmountDue` et `AmountPaid` sont persistés sur la commande ; le montant dû
  reste `null` tant que le total de facture n'a pas été établi.
- `SetPaymentAmounts(amountDue, amountPaid)` définit un instantané validé
  des montants (décimaux non négatifs), côté métier uniquement. Les setters
  privés empêchent leur alimentation par les requêtes de modification de commande.
- `PaymentSummary` expose les montants dû, payé et restant, ainsi que
  `Unpaid`, `PartiallyPaid` ou `Paid` ; il est absent si le montant dû est inconnu.
- Le restant est borné à zéro en cas de trop-perçu, sans perdre le montant payé.
  L'acquittement est atteint si `AmountPaid >= AmountDue`. Une facture
  explicitement établie à zéro est soldée ; un montant inconnu ne l'est pas.
- `GetIsAcquitted()` et le suffixe documentaire `-ack` utilisent ce calcul,
  jamais le seul statut PayPal `SUCCESS`.

La migration ajoute les montants sans reconstituer de paiements historiques :
les anciennes commandes ont un dû inconnu et un payé nul. Un rapprochement
explicite est nécessaire avant de les considérer comme acquittées.
La génération depuis le devis et l'alimentation du payé sont désormais
branchées par le service `InvoicingService` (étapes 2 et 4 ci-dessous).
L'appel `SetPaymentAmounts` remplace un total cumulé ; il n'enregistre pas
une transaction et ne doit pas être utilisé pour additionner un retour PayPal
sans contrôle d'idempotence.

Objectifs couverts :

- identifier le point d'entrée métier de la facture,
- ajouter un modèle de montant dû / montant payé / montant restant,
- distinguer clairement l'état partiel du statut de paiement total,
- éviter toute logique qui force un booléen `IsPaid = true` dès le premier paiement.

### Étape 2 — brancher la génération de facture sur le devis signé

Implémentée par `POST /api/v1/invoices/from-estimate/{estimateId}` :

- réservée au prestataire du devis (ou à un administrateur) ;
- exige les deux dates de validation **et** les deux signatures persistées,
  non vides, associées aux bons signataires ; une date seule ne suffit pas ;
- exige une commande existante, dont le client et le prestataire correspondent
  au devis ; les devis sans commande restent des devis, non des factures ;
- copie les lignes du devis, sa devise EUR, son total et ses parties dans
  un instantané de facture sur la commande, daté côté serveur ;
- accepte les remises mais refuse un total négatif, les quantités non positives,
  les prix infra-centime et les devis multidevises ;
- initialise le payé à zéro, sans assimiler un ancien checkout à un encaissement ;
- renvoie la même facture lors d'un nouvel appel pour le même devis, sans
  changer le total ou effacer les paiements ; une autre facture ou un solde
  historique préexistant sur la commande provoque un conflit de rapprochement.

`GET /api/v1/invoices/{commandId}` expose l'instantané et l'historique aux
parties de la facture et aux administrateurs. Les statuts sont des chaînes
`Unpaid`, `PartiallyPaid` et `Paid`. Les lignes utilisées par `GetBillItems()`
sont celles de la facture figée, même si le devis est modifié ensuite.
Les commandes facturées et leurs devis ne sont plus supprimables par les
routes de commande concernées. Le changement de prestataire est bloqué sur
les routes usuelles de modification ; l'instantané reste la référence des parties.

Cette création établit la facture métier et ses montants, **pas un PDF** :
le moteur historique de rendu documentaire reste distinct. Ses routes de
génération exigent désormais une facture établie.

### Étape 3 — intégrer le mode de contrat du prestataire

- lire le setting du prestataire qui indique le type de contrat,
- associer ce mode à la ligne de frais ajoutée sur la facture,
- garantir que la ligne peut être différenciée selon arrhes ou avance.

### Étape 4 — enregistrer les paiements sans surinterpréter le statut

Implémentée par deux sources d'encaissement, sans débit déclenché par ces routes :

| Route POST sous `/api/v1/invoices/{commandId}` | Source de vérité | Accès |
| --- | --- | --- |
| `/payments/offline` | Attestation du prestataire : référence, montant, devise, date de réception | Prestataire ou administrateur |
| `/payments/paypal` | Réponse serveur PayPal `GetTransactionDetails` pour un identifiant de transaction | Parties ou administrateur |

Le reçu hors ligne doit être strictement positif, en centimes EUR, avec une
référence non vide et une date non future. Il est attesté, pas vérifié auprès
de la banque. La référence est unique par prestataire ; le client ne peut
pas s'auto-déclarer acquitté.

Pour PayPal, seul l'identifiant est accepté du client. Le serveur vérifie :

- statut `Completed`, et non un simple `ACK=Success` ou un statut `Pending` ;
- identifiant exact, référence de facture `TypeCommande/Id` (`INVNUM`),
  devise de la facture et compte marchand destinataire (`RECEIVERID`) ;
- montant brut reçu strictement positif et date retournés par PayPal.

Le token de checkout historique n'est **pas** un identifiant de transaction.
Le retour historique `ConfirmPayment` ne constitue toujours pas une preuve
d'encaissement et n'alimente pas le solde. Cette intégration vérifie des
transactions déjà encaissées ; elle n'ajoute ni capture, ni webhook, ni
automatisation du retour navigateur. Le protocole NVP reprend le système
Classic déjà utilisé par le dépôt ; il est déprécié par PayPal et réservé
aux marchands existants compatibles.

Chaque encaissement conserve sa référence, son origine, son montant, sa
devise, ses dates de réception/enregistrement et l'identifiant de l'acteur.
La référence PayPal est unique globalement : une même transaction ne peut
pas solder deux factures. Rejouer un reçu identique ne l'ajoute pas deux fois ;
réutiliser sa référence avec d'autres détails renvoie `409`.

L'ajout au journal et la mise à jour du total payé sont sauvegardés dans la
même transaction EF relationnelle. Un jeton de concurrence sur la commande
empêche les pertes de mise à jour. En cas de conflit concurrent, la route
renvoie `409` : rejouer **la même référence et le même corps** permet de
réessayer sans double comptage. Un solde ne correspondant pas au journal
requiert un rapprochement, sans réinitialisation silencieuse.

Le solde est calculé à partir des reçus cumulés. Un paiement partiel garde
la facture non acquittée ; un trop-perçu est conservé avec un restant nul.
Le texte de l'addition affiche payé, restant et statut.

#### Configuration et utilisation

Appliquer la migration `SignedEstimateInvoicing` avant déploiement.
Elle ajoute l'instantané, le jeton de concurrence et le journal `InvoicePayments`,
sans créer de facture ni de reçu pour les données historiques.

La vérification PayPal utilise la section `Authentication:PayPal` :
`Mode` (`sandbox` ou `production`), `MerchantAccountId` et
`MerchantAccountUserName`. Une entrée de `Accounts` doit correspondre à ce
dernier via `ApiUsername`, avec `ApiPassword` et `Signature`.
Conserver les identifiants dans les secrets ou l'environnement, jamais dans
un fichier versionné. Sans configuration exploitable, la route renvoie `503`
sans enregistrer de paiement ; une panne ou une réponse invalide PayPal
renvoie `502`/`504`, sans modifier le solde.

Le client partagé `InvoicesApiClient` expose les quatre opérations.
Le document `/openapi/invoicing.json` est réservé aux administrateurs en
développement, comme les autres documents OpenAPI du serveur.
Des exemples de requêtes sont fournis dans `src/Yavsc.Api/Invoicing.http` ;
adapter l'adresse du serveur et les jetons. Aucun écran PostIt n'est ajouté
dans ce lot.

### Étape 5 — finaliser l'état acquitté

- calculer le total payé,
- si `totalPaye >= totalDu`, alors passer la facture en état acquittée,
- envoyer les notifications / documents associés au cas échéant.

Le calcul du statut est branché aux encaissements. Les notifications finales,
le rendu documentaire, les remboursements/annulations de reçus et les
rapprochements historiques restent hors de ce lot, tout comme l'étape 3.
Un remboursement ou litige ultérieur n'est pas synchronisé automatiquement :
ne pas utiliser ce journal d'encaissements comme gestion complète des
remboursements PayPal.

## Tests à couvrir

Les tests implémentés du backend s'exécutent avec le runner MTP du dépôt :

```bash
dotnet test --project src/test/Yavsc.Api.Tests/Yavsc.Api.Tests.csproj \
  --filter-class '*InvoiceApiTests' --filter-class '*HairCutQueryApiControllerTests'
dotnet test --project src/test/Yavsc.Server.Tests/Yavsc.Server.Tests.csproj \
  --filter-class '*PaymentSummaryTests' --filter-class '*PayPalTransactionVerifierTests'
dotnet test --project src/PostIt/PostIt.Tests/PostIt.Tests.csproj \
  --filter-class '*InvoicesApiClientTests'
```

Les tests HTTP utilisent la base relationnelle isolée de la fixture.
Ils vérifient notamment qu'un ajout concurrent perdant est entièrement
annulé (journal et total), puis rejouable. Les tests PayPal utilisent des
réponses HTTP simulées, sans identifiants réels ni appel au prestataire.
Ils ne remplacent pas une recette sandbox sur un compte Classic compatible.

Le flux doit être validé par des tests ciblés sur :

- devis signé par les deux parties → génération d'une facture non acquittée,
- mode arrhes / avance → ligne de frais correctement ajoutée,
- paiement partiel → montant enregistré sans statut payé complet,
- paiement total → facture marquée acquittée,
- addition / message de paiement → mention du montant réellement versé sans faux état d'acquittement complet.

## Critère de sortie fonctionnel

Le comportement attendu est considéré comme atteint si le scénario suivant est vérifié :

1. le devis est doublé signé,
2. le prestataire génère une facture non acquittée,
3. le client paye une avance ou des arrhes,
4. la facture reste non acquittée tant qu'elle n'est pas totalement soldée,
5. la facture devient acquittée uniquement lorsque le total est réglé.

## Note de contexte

Ce document consolide la décision métier issue du workflow “Prestation / devis” et est volontairement limité au périmètre de la facturation et du paiement après signature du devis. Il ne remplace pas les règles plus larges de gestion des litiges, workflows d'annulation ou paiements transverses déjà documentés ailleurs.
