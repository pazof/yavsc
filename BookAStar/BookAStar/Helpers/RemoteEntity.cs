using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace BookAStar
{
    public class RemoteEntity<V,K> : ObservableCollection<V>, ICommand where K : IEquatable<K>
    {
        private string _controller;
        public event EventHandler CanExecuteChanged;
        public bool IsExecuting { get; private set; }
        private HttpClient client;
        private Uri controllerUri;
        protected Func<V, K> GetKey { get; set; }
        public bool CanExecute(object parameter)
        {
            return !IsExecuting;
        }

        public RemoteEntity(string controllerName, Func<V,K> getKey):base()
        {
            if (string.IsNullOrWhiteSpace(controllerName) || getKey == null)
                throw new InvalidOperationException();
            _controller = controllerName;
            GetKey = getKey;
            client = new HttpClient();
            controllerUri = new Uri(MainSettings.YavscApiUrl + "/" + _controller);
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
            // Update credentials 
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", MainSettings.CurrentUser.YavscTokens.AccessToken);
            // Get the whole data
  
            var response = await client.GetAsync(controllerUri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                List<V> col  = JsonConvert.DeserializeObject<List<V>>(content);
                // LocalData.Clear();
                foreach (var item in col)
                {
                    UpdateOrAdd(item);
                }
            }

            AfterExecuting();
        }
        protected virtual void UpdateOrAdd (V item)
        {
            var key = GetKey(item);
            if (this.Any(x => GetKey(x).Equals(key)))
            {
                Remove(LocalGet(key));
            }
            Add(item);
        }

        public V LocalGet(K key)
        {
            return this.Single(x => GetKey(x).Equals(key));
        }

        private void AfterExecuting()
        {
            IsExecuting = false;
            if (CanExecuteChanged != null)
                CanExecuteChanged.Invoke(this, new EventArgs());
        }

        public async Task<V> Get(K key)
        {
           V item = default(V);
           BeforeExecute();
            // Get the whole data
            var uri = new Uri(controllerUri.AbsolutePath+"/"+key.ToString());

            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                item = JsonConvert.DeserializeObject<V>(content);
                // LocalData.Clear();
                    UpdateOrAdd(item);
            }

            AfterExecuting();
            return item;
        }
        
    }
}
