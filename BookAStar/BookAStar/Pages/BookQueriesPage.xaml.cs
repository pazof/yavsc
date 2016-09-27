using BookAStar.Helpers;
using BookAStar.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace BookAStar.Pages
{
    public partial class BookQueriesPage : ContentPage
    {
        public BookQueriesPage()
        {
            InitializeComponent();

            BindingContext = DataManager.Current.BookQueries;
        }
        protected override void OnBindingContextChanged()
        {
            list.ItemsSource = BindingContext as ObservableCollection<BookQueryData>;
            base.OnBindingContextChanged();
        }

        public RemoteEntity<BookQueryData,long> Queries
        {
            get
            {
                return BindingContext!=null? BindingContext as RemoteEntity<BookQueryData, long>:null;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (Queries.CanExecute(null)) Queries.Execute(null);
        }

        private void OnViewDetail(object sender, ItemTappedEventArgs e)
        {
            BookQueryData data = e.Item as BookQueryData;
            App.CurrentApp.ShowBookQuery(data);
        }
    }
}
