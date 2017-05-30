using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Relationship
{
    public class Link
    {
        [Display(Name="Hyper référence")]
        public string HRef { get; set; }

        [Display(Name="Methode Http attendue coté serveur")]
        public string Method { get; set; }

        [Display(Name="Classe de lien")]
        public string Rel { get; set; }

        [Display(Name="Type mime du contenu attendu côté client")]
        public string ContentType { get; set; }

    }
}
