namespace Yavsc.Server.Models.Billing;

public class FrontmatterOffreResult
{
    public string Formulaire { get; set; }
    public string Devis { get; set; }
    public string Corps { get; set; }  // le Markdown sans l'en-tête
}
