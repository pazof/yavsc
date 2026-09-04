namespace Yavsc.Org.Tests.NonRegression;

/// <summary>
/// Guard rails for the SetActivity performer settings page:
/// - countries are provided to the ComboBox via ViewBag.Countries
/// - client-side SIREN validation is wired to country-specific regex rules
/// - controller exposes the validation catalog to the view
/// </summary>
public class SetActivityCountryValidationViewTests
{
    [Fact]
    public void SetActivity_cshtml_binds_country_combo_to_ViewBag_Countries()
    {
        var content = File.ReadAllText(ResolveSetActivityViewPath());

        Assert.Contains("asp-for=\"ExerciseCountryCode\"", content);
        Assert.Contains("asp-items=\"ViewBag.Countries\"", content);
    }

    [Fact]
    public void SetActivity_cshtml_contains_country_aware_siren_javascript_validation()
    {
        var content = File.ReadAllText(ResolveSetActivityViewPath());

        Assert.Contains("$.validator.addMethod(\"sirenByCountry\"", content);
        Assert.Contains("new RegExp(selectedRule.regex)", content);
        Assert.Contains("const countryInput = $(\"#ExerciseCountryCode\")", content);
        Assert.Contains("const sirenInput = $(\"#SIREN\")", content);
    }

    [Fact]
    public void ManageController_exposes_countries_and_country_validation_rules_to_view()
    {
        var content = File.ReadAllText(ResolveManageControllerPath());

        Assert.Contains("ViewBag.Countries = countries;", content);
        Assert.Contains("ViewBag.CountryCodeValidationRules = PerformerCodeInputValidationCatalog.Rules;", content);
        Assert.Contains("ModelState.Remove(nameof(PerformerProfile.ExerciseCountryCode));", content);
    }

    private static string ResolveSetActivityViewPath()
    {
        return ResolveFromWorkspaceRoot(
            "src", "Yavsc.Org", "Views", "Manage", "SetActivity.cshtml");
    }

    private static string ResolveManageControllerPath()
    {
        return ResolveFromWorkspaceRoot(
            "src", "Yavsc.Org", "Controllers", "Accounting", "ManageController.cs");
    }

    private static string ResolveFromWorkspaceRoot(params string[] relative)
    {
        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 10 && dir is not null; i++)
        {
            var candidate = Path.Combine(new[] { dir }.Concat(relative).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = Path.GetDirectoryName(dir);
        }

        throw new FileNotFoundException(
            "Could not locate test target from " + AppContext.BaseDirectory);
    }
}
