namespace Yavsc.Api.Helpers
{
    public static class RequestHelpers
    {
        // Check for some apache proxy header, if any
#pragma warning disable CS8632 // L'annotation pour les types référence Nullable doit être utilisée uniquement dans le code au sein d'un contexte d'annotations '#nullable'.
        public static string? ForwardedFor(this HttpRequest request) {
            string? host = request.Headers["X-Forwarded-For"];
#pragma warning restore CS8632 // L'annotation pour les types référence Nullable doit être utilisée uniquement dans le code au sein d'un contexte d'annotations '#nullable'.
            if (string.IsNullOrEmpty(host)) {
                host = request.Host.Value;
            } else { // Using X-Forwarded-For last address
                host = host.Split(',')
                    .Last()
                    .Trim();
            }
            return host;
        }
    }
}
