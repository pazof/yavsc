yavsc
=====

[doc-fr](http://yavsc.pschneider.fr/Blogs/UserPost/paul/Documentation)

# à faire, par ordre de priorité


1) Créer un contrôle "bouton utilisateur" `UserNameControl`
  avec 
   * un acces rapide à l'ajout au cercles perso
   * pour les administrateur, une action "bloquer",
   * le compteur de ses posts publiques
   * Si c'est un préstataire, et si on est pas déjà dans un formulaire de reservation, 
     un lien vers la reservation de ses services

2) Refabrication de l'Api :

  Concerning the blog entry edition, we only need Two methods: 

  * ```long PostFile(long id)```, 
    used for creation when the given id is 0, in which case, the entry id created is returned.
    Otherwise, used for import in the post spécified by its id, in which case, 0 is returned.
  * `long Post(BlogEntry be)`, used to create or update a given or not 
    blog entry content. the returned value is the entry id at creation, or 0.


3) Corriger un peu le thème clair (fond de titres trop sombre)
