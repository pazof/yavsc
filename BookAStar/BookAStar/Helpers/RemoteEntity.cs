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
    public class LocalEntity<V, K> : ObservableCollection<V> where K : IEquatable<K>
    {
        protected Func<V, K> GetKey { get; set; }
        public LocalEntity(Func<V, K> getKey) : base()
        {
            if (getKey == null) throw new InvalidOperationException();
            GetKey = getKey;
        }
        public virtual void Merge(V item)
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
    }

    public class RemoteEntity<V,K> : LocalEntity<V, K>, ICommand where K : IEquatable<K>
    {
        private string _controller;
        public event EventHandler CanExecuteChanged;
        public bool IsExecuting { get; private set; }
        
        private Uri controllerUri;
        public bool CanExecute(object parameter)
        {
            return !IsExecuting;
        }

        public RemoteEntity(string controllerName, Func<V,K> getKey):base(getKey)
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
            // Update credentials 
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", MainSettings.CurrentUser.YavscTokens.AccessToken);
                // Get the whole data

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
                    }
                }
            }
            AfterExecuting();
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

            using (HttpClient client = new HttpClient())
            {
                using (var response = await client.GetAsync(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        item = JsonConvert.DeserializeObject<V>(content);
                        // LocalData.Clear();
                        Merge(item);
                    }
                }
            }

            AfterExecuting();
            return item;
        }
        
    }
}
