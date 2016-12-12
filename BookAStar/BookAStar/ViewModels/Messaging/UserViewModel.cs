using BookAStar.Helpers;
using BookAStar.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels.Messaging
{
    class UserViewModel : ViewModel
    {
        private ClientProviderInfo data;
        public ClientProviderInfo Data {
            get
            {
                return data;
            }

            set
            {
                SetProperty(ref data, value);
                NotifyPropertyChanged("Avatar");
            }
        }
        private string connexionId;
        public string ConnexionId
        {
            get
            {
                return connexionId;
            }

            set
            {
                SetProperty(ref connexionId, value);
                NotifyPropertyChanged("Connected");
            }
        }
        public bool Connected
        {
            get { return connexionId != null; }
        }

        [JsonIgnore]
        public ImageSource Avatar
        {
            get
            {
                return UserHelpers.Avatar(Data.Avatar);
            }
        }
    }
}
