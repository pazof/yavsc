namespace Yavsc.Domains.Musical;

public static class LicenceModelesSeed
{
    public static IEnumerable<LicenceTemplate> GetSeedData() => new[]
    {
        new LicenceTemplate {
            Id = 1, Nom = "CC0 1.0", IsFreeAndOpen = true,
            PermetUsageCommercial = true, PermetModification = true,
            ExigeAttribution = false, ExigePartageAIdentique = false,
            Texte = "https://creativecommons.org/publicdomain/zero/1.0/",
            AdminValidated = true
        },
        new LicenceTemplate {
            Id = 2, Nom = "CC BY 4.0", IsFreeAndOpen = true,
            PermetUsageCommercial = true, PermetModification = true,
            ExigeAttribution = true, ExigePartageAIdentique = false,
            Texte = "https://creativecommons.org/licenses/by/4.0/",
            AdminValidated = true
        },
        new LicenceTemplate {
            Id = 3, Nom = "CC BY-SA 4.0", IsFreeAndOpen = true,
            PermetUsageCommercial = true, PermetModification = true,
            ExigeAttribution = true, ExigePartageAIdentique = true,
            Texte = "https://creativecommons.org/licenses/by-sa/4.0/",
            AdminValidated = true
        },
        new LicenceTemplate {
            Id = 4, Nom = "CC BY-NC 4.0", IsFreeAndOpen = false,
            PermetUsageCommercial = false, PermetModification = true,
            ExigeAttribution = true, ExigePartageAIdentique = false,
            Texte = "https://creativecommons.org/licenses/by-nc/4.0/",
            AdminValidated = true
        },
        new LicenceTemplate {
            Id = 5, Nom = "CC BY-NC-SA 4.0", IsFreeAndOpen = false,
            PermetUsageCommercial = false, PermetModification = true,
            ExigeAttribution = true, ExigePartageAIdentique = true,
            Texte = "https://creativecommons.org/licenses/by-nc-sa/4.0/",
            AdminValidated = true
        },
    };
}