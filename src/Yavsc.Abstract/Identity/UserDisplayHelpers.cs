namespace Yavsc.Abstract.Identity
{
    /// <summary>
    /// Helpers Razor-friendly pour rendre un user dans une vue
    /// sans exposer le template à des accesseurs nullables qui
    /// lèveraient <see cref="System.NullReferenceException"/>.
    /// </summary>
    public static class UserDisplayHelpers
    {
        /// <summary>
        /// Chemin d'avatar à utiliser pour <paramref name="user"/>
        /// dans un display template. Défense contre <c>null</c>
        /// (user pas chargé, FK orpheline) et contre un
        /// <c>UserName</c> vide (donnée héritée, user partiellement
        /// initialisé). Sans cette garde, Razor émet une
        /// NullReferenceException dès qu'un accesseur <c>.UserName</c>
        /// apparaît dans le template, ce qui remonte en 500 et
        /// masque la page d'erreur elle-même.
        /// </summary>
        /// <remarks>
        /// Le path retourné est aligné sur
        /// <see cref="YavscConstants.AvatarsPath"/> (minuscule).
        /// Les anciens display templates utilisaient "/Avatars/"
        /// avec un S majuscule, en désaccord avec le path statique
        /// servi par le middleware de fichiers — les images ne
        /// résolvaient pas. Centraliser le calcul ici ferme les
        /// deux trous.
        /// </remarks>
        public static string AvatarSrc(IApplicationUser? user)
        {
            if (string.IsNullOrWhiteSpace(user?.UserName))
                return YavscConstants.DefaultAvatar;
            return $"{YavscConstants.AvatarsPath}/{user!.UserName}.s.png";
        }
    }
}
