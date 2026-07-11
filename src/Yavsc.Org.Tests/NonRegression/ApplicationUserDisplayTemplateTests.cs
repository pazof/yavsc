using System.IO;
using Xunit;

namespace Yavsc.Org.Tests.NonRegression;

/// <summary>
/// Régression du 500 sur <c>GET /BlogSpot/Details/{id}</c> (auteur
/// sans <c>UserName</c>) : le display template
/// <c>ApplicationUser.cshtml</c> ne doit plus accéder à
/// <c>Model.UserName</c> directement. Toute lecture passe par
/// <c>UserDisplayHelpers.AvatarSrc</c>, qui défend contre
/// <c>null</c> et contre les chaînes vides/whitespace.
///
/// On ne compile pas la vue Razor ici (coût de mise en place
/// disproportionné pour un seul display template) ; on asserte
/// statiquement que le cshtml ne porte plus l'accès fautif. Si
/// quelqu'un revert la ligne, ce test casse.
/// </summary>
public class ApplicationUserDisplayTemplateTests
{
    [Fact]
    public void ApplicationUser_cshtml_does_not_construct_avatar_path_from_Model_UserName()
    {
        // L'invariant qu'on protège : l'URL d'avatar ne doit plus
        // être construite à partir de Model.UserName direct (le
        // commit 2 du fix). Cette construction était la cause du
        // 500 sur GET /BlogSpot/Details/{id} : avec
        // <Nullable>enable</Nullable>, Razor émet un null-check
        // implicite sur l'expression, et lève NPE si UserName est
        // null. Le helper AvatarSrc défend contre ce cas.
        //
        // On n'interdit pas les autres usages de Model.UserName
        // (alt, title, asp-route-id) : Razor les rend en chaîne
        // vide si null, sans NPE. C'est laid, pas cassé.
        var path = ResolveTemplatePath();
        var content = File.ReadAllText(path);

        // L'ancien code fautif concaténait directement
        // "/Avatars/" + Model.UserName + ".s.png".
        Assert.DoesNotContain("Model.UserName + ", content);
        Assert.DoesNotContain("Model.UserName+", content);
    }

    [Fact]
    public void ApplicationUser_cshtml_uses_the_null_safe_helper_for_avatar()
    {
        var path = ResolveTemplatePath();
        var content = File.ReadAllText(path);

        Assert.Contains("UserDisplayHelpers.AvatarSrc", content);
    }

    private static string ResolveTemplatePath()
    {
        // Le test s'exécute depuis src/Yavsc.Org.Tests/bin/...,
        // on remonte pour trouver la vue source.
        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 8 && dir is not null; i++)
        {
            var candidate = Path.Combine(dir,
                "src", "Yavsc.Org", "Views", "Shared",
                "DisplayTemplates", "ApplicationUser.cshtml");
            if (File.Exists(candidate)) return candidate;
            dir = Path.GetDirectoryName(dir);
        }
        throw new FileNotFoundException(
            "Could not locate ApplicationUser.cshtml from " + AppContext.BaseDirectory);
    }
}
