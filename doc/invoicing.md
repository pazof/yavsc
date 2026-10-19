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
La génération depuis le devis, la fixation du dû et l'alimentation du payé
par des transactions confirmées restent à brancher aux étapes suivantes.
L'appel `SetPaymentAmounts` remplace un total cumulé ; il n'enregistre pas
une transaction et ne doit pas être utilisé pour additionner un retour PayPal
sans contrôle d'idempotence.

Objectifs couverts :

- identifier le point d'entrée métier de la facture,
- ajouter un modèle de montant dû / montant payé / montant restant,
- distinguer clairement l'état partiel du statut de paiement total,
- éviter toute logique qui force un booléen `IsPaid = true` dès le premier paiement.

### Étape 2 — brancher la génération de facture sur le devis signé

- vérifier que le devis est bien doublement validé,
- déclencher la facture depuis le contexte du devis,
- rattacher la facture au bon client / prestataire.

### Étape 3 — intégrer le mode de contrat du prestataire

- lire le setting du prestataire qui indique le type de contrat,
- associer ce mode à la ligne de frais ajoutée sur la facture,
- garantir que la ligne peut être différenciée selon arrhes ou avance.

### Étape 4 — enregistrer les paiements sans surinterpréter le statut

- enregister le paiement effectué,
- alimenter les montants payés et restants,
- ne pas considérer la facture comme totalement acquittée tant que le total n'est pas atteint.

### Étape 5 — finaliser l'état acquitté

- calculer le total payé,
- si `totalPaye >= totalDu`, alors passer la facture en état acquittée,
- envoyer les notifications / documents associés au cas échéant.

## Tests à couvrir

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
