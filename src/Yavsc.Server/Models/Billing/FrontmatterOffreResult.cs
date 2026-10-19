namespace Yavsc.Server.Models.Billing;

public class FrontmatterOffreResult
{
    public string Formulaire { get; set; } = string.Empty;
    public string Devis { get; set; } = string.Empty;
    public string Corps { get; set; } = string.Empty;  // le Markdown sans l'en-tête
}
