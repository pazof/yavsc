
using Newtonsoft.Json;
using System;

namespace BookAStar.Data.NonCrUD
{
    using Helpers;
    using Model.FileSystem;

    public class RemoteFiles : RemoteEntity<UserDirectoryInfo, FileAddress>
    {
        public RemoteFiles() : base("fs", d => d)
        {

        }

        public override async void Execute(object parameter)
        {
            BeforeExecute();
            using (var client = UserHelpers.CreateClient())
            {
                // Get the whole data
                try
                {
                    var subpath = parameter as string;
                    string path = ControllerUri.AbsolutePath + ((subpath != null) ? "/" + subpath : null);
                    using (var response = await client.GetAsync(ControllerUri))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            var di = JsonConvert.DeserializeObject<UserDirectoryInfo>(content);
                            this.Merge(di);
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                            throw new Exception("Bad request");
                        else 
                            throw new Exception("Remote call failed");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Remote call failed", ex);
                }
            }
        }

       
    }
}
