using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace Yavsc.WebApiClient.Helpers;

public static class HttpContextHelpers
{
    public static async Task<string> GetJson(this HttpContext httpContext, string endPoint)
        {
            var accessToken = await httpContext.GetTokenAsync("access_token");
            var client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var content = await client.GetAsync(endPoint);
            content.EnsureSuccessStatusCode();
            var json = await content.Content.ReadAsStringAsync();
            return json;
        }
    
}
