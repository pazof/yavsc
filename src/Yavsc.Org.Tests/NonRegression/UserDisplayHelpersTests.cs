using Xunit;
using Yavsc.Abstract;
using Yavsc.Abstract.Identity;

namespace Yavsc.Org.Tests.NonRegression;

/// <summary>
/// Régression sur le 500 <c>GET /BlogSpot/Details/{id}</c> : un
/// billet dont l'auteur a un <c>UserName</c> null ou absent faisait
/// lever <c>NullReferenceException</c> dans le display template
/// <c>ApplicationUser.cshtml</c> à l'évaluation de
/// <c>Model.UserName</c>. ASP.NET transforme en 500, et la page
/// d'erreur elle-même crash (ErrorViewModel manquant), donc on
/// ne voit rien — juste un 500 muet.
///
/// Le fix passe par <see cref="UserDisplayHelpers.AvatarSrc"/> qui
/// retourne <see cref="YavscConstants.DefaultAvatar"/> pour toute
/// donnée partielle. Ces tests couvrent les trois formes de
/// "donnée absente" : user null, UserName vide, UserName whitespace.
/// </summary>
public class UserDisplayHelpersTests
{
    [Fact]
    public void AvatarSrc_null_user_returns_default_avatar()
    {
        Assert.Equal(YavscConstants.DefaultAvatar, UserDisplayHelpers.AvatarSrc(null));
    }

    [Fact]
    public void AvatarSrc_user_with_empty_UserName_returns_default_avatar()
    {
        var user = new FakeUser { UserName = "" };
        Assert.Equal(YavscConstants.DefaultAvatar, UserDisplayHelpers.AvatarSrc(user));
    }

    [Fact]
    public void AvatarSrc_user_with_whitespace_UserName_returns_default_avatar()
    {
        var user = new FakeUser { UserName = "   " };
        Assert.Equal(YavscConstants.DefaultAvatar, UserDisplayHelpers.AvatarSrc(user));
    }

    [Fact]
    public void AvatarSrc_valid_user_returns_canonical_avatars_path()
    {
        var user = new FakeUser { UserName = "alice" };
        // Le path doit matcher YavscConstants.AvatarsPath (minuscule),
        // pas un /Avatars/ avec S majuscule qui ne résout pas
        // dans le middleware de fichiers statiques.
        var expected = $"{YavscConstants.AvatarsPath}/alice.s.png";
        Assert.Equal(expected, UserDisplayHelpers.AvatarSrc(user));
    }

    private sealed class FakeUser : IApplicationUser
    {
        public string Id { get; set; } = "";
        public string? UserName { get; set; }
        public string? Avatar { get; set; }
        public IAccountBalance? AccountBalance => null;
        public string? DedicatedGoogleCalendar => null;
        public ILocation? PostalAddress => null;
    }
}
