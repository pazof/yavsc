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

    public class RemoteEntity<V, K> : LocalEntity<V, K>, ICommand where K : IEquatable<K>
    {
        private string _controller;
        public event EventHandler CanExecuteChanged;
        public bool IsExecuting { get; private set; }

        private Uri controllerUri;
        public bool CanExecute(object parameter)
        {
            return !IsExecuting && (MainSettings.CurrentUser != null);
        }

        public RemoteEntity(string controllerName, Func<V, K> getKey) : base(getKey)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
                throw new InvalidOperationException();
            _controller = controllerName;
            controllerUri = new Uri(Constants.YavscApiUrl + "/" + _controller);
        }

        private void BeforeExecute()
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
        public async void Execute(object parameter)
        {
            BeforeExecute();
            using (HttpClient client = UserHelpers.CreateClient())
            {
                // Get the whole data
                try
                {
                    using (var response = await client.GetAsync(controllerUri))
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

        private void AfterExecuting()
        {
            IsExecuting = false;
            if (CanExecuteChanged!=null)
                CanExecuteChanged.Invoke(this, new EventArgs());
        }

        public async Task<V> Get(K key)
        {
            var item = LocalGet(key);
            if (item == null) item = await RemoteGet(key);
            CurrentItem = item;
            return CurrentItem;
        }

        public async Task<V> RemoteGet(K key)
        {
            V item = default(V);
            BeforeExecute();
            // Get the whole data
            var uri = new Uri(controllerUri.AbsoluteUri + "/" + key.ToString());

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

            AfterExecuting();
            CurrentItem = item;
            return item;
        }

        public async void Create(V item)
        {
            BeforeExecute();

            using (HttpClient client = UserHelpers.CreateClient())
            {
                HttpContent content = new StringContent(
                    JsonConvert.SerializeObject(item)
                    );
                using (var response = await client.PostAsync(controllerUri, content))
                {
                    if (!response.IsSuccessStatusCode)
                        // TODO throw custom exception, and catch to inform user
                        throw new Exception($"Create failed posting {item} @ {controllerUri.AbsolutePath}");
                    var recontent = await response.Content.ReadAsStringAsync();
                    JsonConvert.PopulateObject(recontent, item);

                }
            }

            AfterExecuting();
            CurrentItem = item;
        }

    }
}
