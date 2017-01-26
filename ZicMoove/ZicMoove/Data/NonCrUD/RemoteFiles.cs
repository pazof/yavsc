
using Newtonsoft.Json;
using System;

namespace BookAStar.Data.NonCrUD
{
    using Helpers;
    using Model.FileSystem;

    public class RemoteFilesEntity : RemoteEntity<UserDirectoryInfo, FileAddress>
    {
        public RemoteFilesEntity() : base("fs", d => d)
        {

        }

        public override async void Execute(object parameter)
        {
            BeforeExecute();
            using (var client = UserHelpers.CreateJsonClient())
            {
                // Get the whole data
                try
                {
                    var subpath = parameter as string;
                    string path = ControllerUri.AbsoluteUri + ((subpath != null) ? "/" + subpath : null);
                    using (var response = await client.GetAsync(path))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            var di = JsonConvert.DeserializeObject<UserDirectoryInfo>(content);
                            this.Merge(di);
                            this.SaveEntity();
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
            AfterExecuting();
        }

        /*
        public override void Merge(UserDirectoryInfo item)
        {
            var key = GetKey(item);
            DirectoryEntryChangingEvent itemChanged = null;
            if (this.Any(x => GetKey(x).Equals(key)))
            {
                var old = LocalGet(key);
                itemChanged = new DirectoryEntryChangingEvent
                {
                    OldItem = old,
                    NewItem = item
                };
                Remove(old);
            }
            Add(item);
            CurrentItem = item;
            if (DirectoryEntryChanged != null && itemChanged != null)
                DirectoryEntryChanged.Invoke(this, itemChanged);
        }

        public event EventHandler<DirectoryEntryChangingEvent> DirectoryEntryChanged;
        */

    }
}
