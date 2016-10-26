using BookAStar.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace BookAStar.Views
{
    public partial class UserListView : ContentView
    {
        public BindableProperty ItemsSourceProperty = BindableProperty.Create(
            "ItemsSource", typeof(IEnumerable), typeof(UserListView));

        public IEnumerable ItemsSource
        {
            get { return list.ItemsSource; }
            set { list.ItemsSource = value; }
        }

        /*        public IEnumerable ItemsSource
        {
            get { return GetValue(ItemsSourceProperty) as IEnumerable; }
            set { SetValue(ItemsSourceProperty, value); }
        }
        */
        public UserListView()
        {
            InitializeComponent();
        }

    }
}
