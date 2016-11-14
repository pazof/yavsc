using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net;

namespace BookAStar.Data
{
    using Helpers;
    using System.Diagnostics;
    using System.Text;

    public class RemoteEntity<V, K> : LocalEntity<V, K>, ICommand where K : IEquatable<K>
    {
        public string ControllerName { protected set; get; }
        public event EventHandler CanExecuteChanged;
        public bool IsExecuting { get; private set; }

        public Uri ControllerUri { get; protected set; }

        public bool CanExecute(object parameter)
        {
            return !IsExecuting && (MainSettings.CurrentUser != null);
        }

        public RemoteEntity(string controllerName, Func<V, K> getKey) : base(getKey)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
                throw new InvalidOperationException();
            ControllerName = controllerName;
            ControllerUri = new Uri(Constants.YavscApiUrl + "/" + ControllerName);
        }

        protected void BeforeExecute()
        {
            if (IsExecuting)
                throw new InvalidOperationException("Already executing");
            IsExecuting = true;
            if (CanExecuteChanged != null)
                CanExecuteChanged.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Refresh the collection
        /// </summary>
        /// <param name="parameter"></param>
        public virtual async void Execute(object parameter)
        {
            BeforeExecute();
            using (HttpClient client = UserHelpers.CreateClient())
            {
                // Get the whole data
                try
                {
                    using (var response = await client.GetAsync(ControllerUri))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            List<V> col = JsonConvert.DeserializeObject<List<V>>(content);
                            // LocalData.Clear();
                            foreach (var item in col)
                            {
                                Merge(item);
                            }
                            this.SaveCollection();
                        }
                    }
                }
                catch (WebException webex)
                {
                    throw new ServiceNotAvailable("No remote entity", webex);
                }

            }
            AfterExecuting();
        }

        protected void AfterExecuting()
        {
            IsExecuting = false;
            if (CanExecuteChanged != null)
                CanExecuteChanged.Invoke(this, new EventArgs());
        }

        public virtual async Task<V> Get(K key)
        {
            var item = LocalGet(key);
            if (item == null) item = await RemoteGet(key);
            CurrentItem = item;
            return CurrentItem;
        }

        protected Uri GetUri(K key)
        {
            return new Uri(ControllerUri.AbsoluteUri + "/" + key.ToString());
        }

        public virtual async Task<V> RemoteGet(K key)
        {
            V item = default(V);
            BeforeExecute();
            // Get the whole data
            var uri = GetUri(key);

            using (HttpClient client = UserHelpers.CreateClient())
            {
                using (var response = await client.GetAsync(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        item = JsonConvert.DeserializeObject<V>(content);
                        Merge(item);
                    }
                }
            }

            CurrentItem = item;
            AfterExecuting();
            return item;
        }

        public virtual async void Create(V item)
        {
            BeforeExecute();

            using (HttpClient client = UserHelpers.CreateClient())
            {
                var stringContent = JsonConvert.SerializeObject(item);
                HttpContent content = new StringContent(
                    stringContent, Encoding.UTF8, "application/json"
                    );
                using (var response = await client.PostAsync(ControllerUri, content))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        // TODO throw custom exception, and catch to inform user
                        var errcontent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"Create failed posting {stringContent} @ {ControllerUri.AbsoluteUri}: {errcontent}");
                    }
                    else
                    {
                        var recontent = await response.Content.ReadAsStringAsync();
                        JsonSerializerSettings sett = new JsonSerializerSettings();
                        JsonConvert.PopulateObject(recontent, item);
                    }
                }

            }

            CurrentItem = item;
            AfterExecuting();
        }
        public virtual async void Update(V item)
        {
            BeforeExecute();

            var uri = GetUri(GetKey(item));
            using (HttpClient client = UserHelpers.CreateClient())
            {
                HttpContent content = new StringContent(
                    JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json"
                    );
                using (var response = await client.PutAsync(uri, content))
                {
                    if (!response.IsSuccessStatusCode)
                    {// TODO throw custom exception, and catch to inform user
                        if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            var recontent = await response.Content.ReadAsStringAsync();
                        }
                        
                        else Debug.WriteLine($"Update failed ({item} @ {uri.AbsolutePath} )");

                    }
                    else
                    {
                        // TODO implement an handler of string content (json)
                        // in most of cases, nothing to do
                        var recontent = await response.Content.ReadAsStringAsync();
                        JsonConvert.PopulateObject(recontent, item);
                    }
                }
            }

            CurrentItem = item;
            AfterExecuting();
        }

        public virtual async void Delete(K key)
        {
            BeforeExecute();
            var uri = GetUri(key);
            using (HttpClient client = UserHelpers.CreateClient())
            {
                using (var response = await client.DeleteAsync(uri))
                {
                    if (!response.IsSuccessStatusCode)
                        // TODO throw custom exception, and catch to inform user
                        throw new Exception($"Delete failed @ {uri.AbsolutePath}");
                }
            }
            CurrentItem = default(V);
            AfterExecuting();
        }
    }
}
