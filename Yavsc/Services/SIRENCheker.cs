using System.Net.Http;
using System.Threading.Tasks;
using Yavsc.Helpers;

namespace Yavsc.Services
{
    using Models.societe.com;
    public class SIRENChecker
    {
        private CompanyInfoSettings _settings;
        public SIRENChecker(CompanyInfoSettings settings)
        {
            _settings = settings;
        }
        public async Task<CompanyInfoMessage> CheckAsync(string siren) {
             using (var web = new HttpClient())
                {
                   return await web.CheckSiren(siren, _settings);
                }
        }
    }
}
