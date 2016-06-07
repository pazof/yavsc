
# localisation:

## Implémentation

* Implementer l'ActionFilter, qui regarde l'entête `accept-language`
* Initier l'espace de nom de la localisation avec la classe vide
* Ecrire les ressources

## Usage 

* Enregistrer le filtre ala scoped
* Décorer le controleur avec l'attribut `[ServiceFilter(typeof(LanguageActionFilter))]`
* Injection de la ressource
* Extensions de nom de fichier (ex: /Home/Index.fr.cshtml)

