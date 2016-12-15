using BookAStar.Model;
using BookAStar.ViewModels.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace BookAStar.Views
{
    public partial class UserListView : ContentView
    {
        public BindableProperty ItemsSourceProperty = BindableProperty.Create(
            "ItemsSource", typeof(ChatUserCollection), typeof(UserListView), default(ChatUserCollection),
            BindingMode.OneWay);

        public BindableProperty ItemSelectedProperty = BindableProperty.Create(
            "ItemSelected", typeof(ICommand), typeof(UserListView), default(ICommand));

        public BindableProperty DisableSelectionProperty = BindableProperty.Create(
            "DisableSelection", typeof(bool), typeof(UserListView), false);

        public ChatUserCollection ItemsSource
        {
            get { return (ChatUserCollection) GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        
        public ICommand ItemSelected
        {
            get { return (ICommand) GetValue(ItemSelectedProperty); }
            set { SetValue(ItemSelectedProperty, value); }
        }

        public bool DisableSelection
        {
            get { return (bool) GetValue(DisableSelectionProperty); }
            set { SetValue(DisableSelectionProperty, value); }
        }

        public UserListView()
        {
            InitializeComponent();
            list.ItemSelected += OnUserSelected;

        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            ICommand dataCommand = (ICommand) BindingContext;
            list.RefreshCommand = new Command(
                () => {
                    dataCommand.Execute(null);
                    list.EndRefresh();
                });
        }
        public void OnUserSelected(object sender, SelectedItemChangedEventArgs ev)
        {
            if (ItemSelected != null)
                if (ItemSelected.CanExecute(ev.SelectedItem))
                {
                    ItemSelected.Execute(ev.SelectedItem);
                }
            if (DisableSelection) list.SelectedItem = null;
        }
    }
}
