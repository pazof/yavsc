
namespace ZicMoove.Data
{
    using Helpers;
    using Model.Workflow;
    using Newtonsoft.Json;
    using Settings;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class EstimateEntity : RemoteEntity<Estimate, long> 
    {
        public EstimateEntity() : base("estimate", e => e.Id)
        {
        }

        public async Task SignAsProvider(Estimate estimate, Stream stream)
        {

            if (estimate.Id == 0)
            {
                    if (!await this.Create(estimate))
                    {
                       await App.DisplayAlert("Erreur d'accès au serveur", "Echec de l'envoi de l'estimation");
                    }
            }
            using (HttpClient client = UserHelpers.CreateJsonClient())
            {
                try
                {
                    var requestContent = new MultipartFormDataContent();
                    var content = new StreamContent(stream);
                    var filename = $"prosign-{estimate.Id}.png";
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    content.Headers.Add("Content-Disposition", $"form-data; name=\"file\"; filename=\"{filename}\"");
                    requestContent.Add(content, "file", filename);
                   
                    using (var response = await client.PostAsync(
                    Constants.YavscApiUrl + $"/pdfestimate/prosign/{estimate.Id}", requestContent))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            var errContent = await response.Content.ReadAsStringAsync();
                            await App.DisplayAlert("SignAsProvider", $"{response.StatusCode}: {errContent}");
                        }
                        else
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            JsonConvert.PopulateObject(json, estimate);
                            this.Add(estimate);
                            this.SaveEntity();
                        }
                    }
                }
                catch (Exception ex)
                {
                    await App.DisplayAlert("SignAsProvider", ex.Message);
                }
            }
        }

        // TODO Check we don't loose nothing here
        public override void Merge(Estimate item)
        {
            var key = GetKey(item);
            if (this.Any(x => GetKey(x).Equals(key)))
            {
                Remove(LocalGet(key));
            }
            Add(item);
            CurrentItem = item;
        }

    }
}
