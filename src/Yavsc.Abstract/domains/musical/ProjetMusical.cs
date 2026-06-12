namespace Yavsc.Domains.Musical;

public class ProjetMusical
{
    public long Id { get; set; }
    public string Titre { get; set; }
    public long LicenceModeleId { get; set; }
    public LicenceTemplate Licence { get; set; }
    // ... contributeurs, fichiers, devis liés
}