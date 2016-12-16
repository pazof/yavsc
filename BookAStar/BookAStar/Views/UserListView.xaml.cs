using BookAStar.Model;
using BookAStar.Model.Social.Chat;
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

        public BindableProperty ItemSelectedCommandProperty = BindableProperty.Create(
            "ItemSelectedCommand", typeof(ICommand), typeof(UserListView), default(ICommand));

        public BindableProperty DisableSelectionProperty = BindableProperty.Create(
            "DisableSelection", typeof(bool), typeof(UserListView), false);

        public BindableProperty HasASelectionProperty = BindableProperty.Create(
            "HasASelection", typeof(bool), typeof(UserListView), false);
        
        public BindableProperty SelectedUserProperty = BindableProperty.Create(
            "SelectedUser", typeof(ChatUserInfo), typeof(UserListView), default(ChatUserInfo));

        public ChatUserCollection ItemsSource
        {
            get { return (ChatUserCollection) GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public ChatUserInfo SelectedUser
        {
            get
            {
                return (ChatUserInfo) GetValue(SelectedUserProperty);
            }
        }

        public ICommand ItemSelectedCommand
        {
            get { return (ICommand) GetValue(ItemSelectedCommandProperty); }
            set { SetValue(ItemSelectedCommandProperty, value); }
        }

        public bool DisableSelection
        {
            get { return (bool) GetValue(DisableSelectionProperty); }
            set { SetValue(DisableSelectionProperty, value); }
        }

        public bool HasASelection
        {
            get { return (bool)GetValue(HasASelectionProperty); }
            set { SetValue(HasASelectionProperty, value); }
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
            if (ItemSelectedCommand != null)
                if (ItemSelectedCommand.CanExecute(ev.SelectedItem))
                {
                    ItemSelectedCommand.Execute(ev.SelectedItem);
                }
            if (DisableSelection)
                list.SelectedItem = null;
            SetValue(SelectedUserProperty, list.SelectedItem);
            HasASelection = list.SelectedItem != null;
        }

    }
}
