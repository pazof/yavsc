namespace Yavsc.Domains.Musical;
public class LicenceTemplate
{
    public long Id { get; set; }
    public string Nom { get; set; }
    public string Texte { get; set; }        // ou Uri vers le texte officiel
    public bool EstLibre { get; set; }
    public bool PermetUsageCommercial { get; set; }
    public bool PermetModification { get; set; }
    public bool ExigePartageAIdentique { get; set; }  // ShareAlike
    public bool ExigeAttribution { get; set; }         // BY
    public bool AdminValidated { get; set; }
    public DateTimeOffset DateValidation { get; set; }
    public string AdminValidateurId { get; set; }      // FK vers ApplicationUser
}
