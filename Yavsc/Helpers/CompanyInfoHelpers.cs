using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yavsc.Model.societe.com;

namespace  Yavsc.Helpers
{
    public static class ComapnyInfoHelpers { 
        public static async Task<CompanyInfoMessage> CheckSiren(this HttpClient web,
        string siren, CompanyInfoSettings api)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get,
            string.Format(Constants.CompanyInfoUrl,siren,api.ApiKey))) {
                using (var response = await web.SendAsync(request)) {
                    var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                    return payload.ToObject<CompanyInfoMessage>();
                }
            }
        }
    }
}