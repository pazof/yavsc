
namespace ZicMoove.Data
{
    using Helpers;
    using Model.Workflow;
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;

    public class EstimateEntity : RemoteEntity<Estimate, long> 
    {
        public EstimateEntity() : base("estimate", e => e.Id)
        {
        }

        public async void SignAsProvider(Estimate estimate, Stream stream)
        {

            if (estimate.Id == 0)
            {
                var ok = await this.Create(estimate);
                if (!ok)
                {
                    await App.DisplayAlert("Erreur d'accès au serveur", "Echec de l'envoi de l'estimation");
                    return;
                }
                this.Add(estimate);
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
                            throw new ApiCallFailedException($"SignAsProvider: {response.StatusCode} / {errContent}");
                        }
                        var json = await response.Content.ReadAsStringAsync();
                        JsonConvert.PopulateObject(json, estimate);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            this.SaveEntity();
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
