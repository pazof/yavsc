yavsc
=====

[doc-fr](http://yavsc.pschneider.fr/Blogs/UserPost/paul/Documentation)

# à faire, par ordre de priorité

0) Concevoir ou pas un objet Contact listant des point d'accès (email, http, sip, irc, téléphone, adresse postale ...)

1) Créer un contrôle "bouton utilisateur" `UserNameControl`
  avec 
   * un acces rapide à l'ajout au cercles perso
   * pour les administrateur, une action "bloquer",
   * le compteur de ses posts publiques
   * Si c'est un préstataire, et si on est pas déjà dans un formulaire de reservation, 
     un lien vers la reservation de ses services

2) Terminer l'édition du profile de site, avec la modification
  et la suppression des activités et compétences


3) Refabrication de l'Api :

  concernant la mise à jour la creation et l'edition d'un post, on
  doit pouvoir fondre tout en une seule methode : 

  * ```long PostFile(BlogEntry be)```, 
    Utilisée pour la creation quand id est à 0, auquel cas, l'identiffiant
    du post créé est renvoyé en retour (non nul).
    Sinon, c'est une mise a jour des propriétés
    du billet, et on renvoie zero.

    Dans tous les cas, toutes les propriétés du post sont fournies car mises à jour, 
    et on effectue la reception des fichiers attachés.

    Dans le cas de l'edition (id non nul), 
    seules les propriétés spécifiées non nulles sont mises à jour
    (NDLR:la visibilité est donc par exemple toujours mis à jour).

4) Messagerie instantanée

6) Tester le premier client Android xamarin 

5) Tester le premier client Android-java libre

7) Tester le premier client iOS xamarin 
