# Yavsc.vNext

Da road to the hell

Ceci est une grosse liste de fonctionnalités, existantes, ou à implémenter, ou à oublier ...

## Jalon 1

	☐ Des spécifications détaillées du coeur de l'application
	✔ Acces (publique) aux Blogs. @done (August 13th 2016, 0:51)
	✔ Accès aux profiles des intervenants. @done (August 13th 2016, 0:57)
	✔ Demande de devis, spécifiant une date et un lieu, envers un pro (ou "BookQuery", ou "demande de rendez-vous"). @done (August 13th 2016, 0:57)
	✔ Notifications commande. @done (September 15th 2016, 15:00)
	✔ Rendu Html du format Markdown-av (+audio&video). @done (October 6th 2016, 14:32)
	✔ Rendu Android, via WebKit WebView. @done (October 6th 2016, 14:32)
    ✔ Salon public. @done (September 28th 2016, 17:58)
	✔ Saisie et soumission basique du devis. @done (October 30th 2016, 21:50)
	✔ Devis au formats TeX et Pdf en ligne à accès restreint. @done (November 6th 2016, 2:10)
	✔ Idem depuis l'API web. @done (November 10th 2016, 10:00)
	✔ Signature du devis par le fournisseur (l'arstiste), depuis le mobile. @done (December 7th 2016, 10:45)
	☐ Au devis validé par le fournisseur pour le client : Envoi d'une notification au client et mise à disposition du devis signé
	* à l'accord du client sur le devis :
		☐ Envoi d'une notification au fournisseur et mise à disposition de la signature client et d'un commentaire.
		☐ Edition de la facture non aquittée depuis le mobile.
		☐ Edition de contrat depuis le mobile.
		☐ Notification et mise à disposition du contrat pour le client.
		☐ Signature de contrat par le client.
		☐ Notification au client de la facture non aquittée.
		(+ ☐ Paiement du client?)
		☐ Edition de la facture marquée aquitée et signée par le fournisseur.
		☐ Notification au client de la facture aquittée signée.
    ✔ Chat privé (coté serveur). @done (October 13th 2016, 16:27)
	✔ Accès mobile au salon public. @done (October 13th 2016, 16:27)
	✔ Accès Web au chat privé. @done (November 3rd 2016, 14:57)
	✔ Accès mobile au chat privé. @done (November 3rd 2016, 11:15)
	✔ Envoi de l'avatar. @done (December 7th 2016, 10:45)
	✔ Envoi de l'avatar depuis le mobile. @done (May 31st 2017, 3:19)
	* Gestion du profile utilisateur:
		✔ Simple [Web] @done (May 31st 2017, 3:19)
		✔ Prestataire [Web] @done (May 31st 2017, 3:19)
		☐ Simple [Android]
		☐ Prestataire [Android]
	* Gestion de l'espace de stockage personnel: 
		☐ Depuis le Web.
		☐ Depuis l'API web.
		☐ Depuis le Mobile.
	☐ Gestion de l'espace de stockage personnel, depuis l'API Web. 
	✔ Envoi de fichiers depuis de Web. @done (December 7th 2016, 10:45)
	☐ Évaluation de la prestation.
	☐ Creation d'une annonce
	☐ Transformation d'une annonce en demande nominative, et inversement

	☐ Page d'accueil du mobile: Les alertes, et les derniers événements, donc, les derniers post, les 
	dernières inscriptions de prestataire, les annonces
	* La sélection d'une star: 
		☐ delegation du traitement des demandes
		☐ depuis le mobile
		✔ depuis le Web @done (May 31st 2017, 3:25)
	* La sélection d'un DJ: (rechercher dans / afficher) les samples + idem chanteur
	  les critères : la date, les champs du formulaire de commande: le message, le lieu, la tendance musicale
		☐ depuis le mobile
		✔ depuis le Web @done (September 25th 2017, 17:31)
	* La sélection d'un chanteur: (rechercher dans / afficher) les titres du répertoire
		☐ depuis le mobile
		✔ depuis le Web @done (September 25th 2017, 17:31)
	* La sélection d'une formation musicale: idem chanteur + (rechercher/afficher) la taille de la formation
		☐ depuis le mobile
		✔ depuis le Web @done (September 25th 2017, 17:31)
	* Géocodeur de lieu
		✔ Web @done (May 31st 2017, 3:20)
		☐ Mobile
	* Géocodeur adresse postale
		☐ Web
		☐ Mobile
	☐ Evaluation des trajets
	✔ Réactivation des cercles privés (cassé avec vNext) @done (May 31st 2017, 3:22)
	✔ Support multi-lanque de l'app mobile @done (December 7th 2016, 10:58)
	✔ Support multi-lanque du web @done (December 7th 2016, 10:58)

## Jalon 2


	☐ Interface de gestion des collaborateurs
	☐ Migration de l'identifiant utilisateur, de la chaine de caractère ala GUID vers l'entier long auto incrémenté par Npgsql 

	☐ Login with Twitter
	☐ Login with PayPal

	☐ Contrôle Web du rating

	☐ Paiement client d'un approvisionnement pour une demande de prestation définie.
	☐ La saisie et l'usage des disonibilités, les ponctuelles, les réccurentes, et les périodes de congé.
	☐ Notifications structurelles (liste de notifications impératives, relatives au site, 
	spécifiées par groupe d'utilisateurs).
	☐ Notifications aux posts, à l'entée d'un artiste, à la conclusion d'une success story.
	☐ Construction du droit à l'envoi d'un message privé, spécifié par le destinataire 
	et/ou par accréditation administrative, à un utilisateur spécifié, ou
	à un cercle ou à un groupe, de manière temporaire ou définitive, par une plage de temps spécifié
	ou par la validité d'une demande de devis ou une intervention en cours ou récente ou à venir.
	☐ Paiement client du reste de la prestation.
	☐ Podcasts.
	☐ Personalisation des blogs.
	☐ Monétarisations.
	☐ Distribution de gold card (illimité), de carte vertes (à durée fixe), 
	de la part d'un prestataire, donnant accès au destinataire au chat privé.
	☐ La carte blanche: en la confiant, le prestataire délègue la gestion de son agenda à un autre utilisateur, 
	elle implique le droit au chat privé
	☐ de badges temporaires, donnant accès à un téléchargement unique et limité dans le temps, d'un seul fichier, 
	sans autre forme d'autorisation.
	
	### Réécritures prévues : 

	☐ Eviter la création de fichier TeX inutile à la génération du Pdf
	
	### Accessibilité :
	☐ Correction des libéllés manquant et/ou en anglais relevés avant le jalon 2 
	
	### Sécurité
	☐ Quota fs utilisateur
	☐ Usage de clés anti-forge coté interfaces Web 
	☐ Fixer "là où ça manque" les limitations en taille des chaines de caratères du modèle 
	
	### Configurabilité
	☐ Gestion de contenu des pages du site, au format interne (NRLD: i.e. non exposé,
	absent du code livré aux clients Web) Markdown-av (`@Html.SiteContent<AccessRules>(id).Responsible`)
	
## Jalon 3

	☐ Saisie du devis sur commande générique: les commandes sont des projets utilisateur associés
	à une liste de formulaire de prise de commande auprès de services que le producteur peut spécialiser,
	par l'ajout de groupes de champs de saisie complètement définis : 
	☐ Chaque champ possède:
		☐ Un label
		☐ Un type pour la valeur
		☐ Un type de controle
		☐ Une liste de validateurs
		☐ Une invitation à la saisie
		 
	☐ Chaque champ apparait dans un groupe de champ possèdant un titre
	☐ Saisie d'un devis à destination d'un invité (envoi par email, contact local)
	☐ Paiement d'Arrhes

## Jalon 4

	☐ Aide à la rencontre physique, avec une carte interactive des positions 
	des participants, du geofencing pour les alerter de la rencontre, et plus si affinités. 
	☐ Activités secondaires: Une activité peut se spécialiser, on le materialise dans le modèle
	en l'enregistrant comme activité fille. Par ailleurs, l'utilisateur peut déclarer plusieurs activités,
	sous la forme d'une liste, dont le premier élément indique l'activité principale 
	(pour laquelle un code APE doit correspondre).
	☐ Le streaming vidéo, de pair à pair
	☐ Le streaming vidéo public.

## Jalon 5

	☐ Conciliation: Une aide à la décision en cas de conflit entre fournisseur et client: 
	à partir du dossier de la commande, on peut aider, en tant que personne du milieu, qui a accès aux deux vues du dossier, 
	celle du client et celle du fournisseur, des indicateurs peuvent aider un membre de l'équipe commerciale (du groupe "FrontOffice")
	à choisir où se place la part de chacun dans l'échec du projet, et à en faire part au bélligérants dans l'espoir d'un accord.
	☐ Le réseau pro : on a des sous-traitants, on leur transmet des demandes de devis en tant que sous-traitance
	d'une demande en cours, cette dernière n'est validable qu'une fois seulement que tous les devis dont elle dépend sont validés.
