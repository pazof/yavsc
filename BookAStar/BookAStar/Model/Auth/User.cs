using BookAStar.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BookAStar.Model.Auth.Account
{ 
    public class User : INotifyPropertyChanged
    {
        private string id;
        public string Id {
            get {
                return id;
            }

            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }
        private string userName;
        public string UserName {

            get
            {
                return userName;
            }

            set
            {
                userName = value;
                OnPropertyChanged("UserName");
            }
        }

        private IEnumerable<string> eMails;
        
        public IEnumerable<string> EMails
        {
            get { return eMails;  }

            set
            {
                eMails = value;
                OnPropertyChanged("EMails");
            }
        }
        private IEnumerable<string> roles;

        public IEnumerable<string> Roles
        {
            get
            {
                return roles;
            }

            set
            {
                roles = value;
                OnPropertyChanged("Roles");
            }
        }
        private string avatar;
        public string Avatar
        {
            get
            {
                return avatar;
            }

            set
            {
                avatar = value;
                OnPropertyChanged("Avatar");
            }
        }
        [JsonIgnore]
        public ImageSource AvatarSource
        {
            get
            {
                return UserHelpers.Avatar(avatar);
            }
        }
        private Tokens yavscTokens;
        public Tokens YavscTokens
        {
            get
            {
                return yavscTokens;
            }

            set
            {
                yavscTokens = value;
                OnPropertyChanged("YavscTokens");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
