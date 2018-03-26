namespace Yavsc.Server.Helpers
{
    public static class DbHelpers
    {
        // FIXME BUG [fr] DependencyInjection Pourquoi ce champ ne pourrait pas devenir une propriété ?
        // : casse à l'execution d'un controlleur, se plaignant d'une valeur nule
        public static string ConnectionString  = null;
    }
}