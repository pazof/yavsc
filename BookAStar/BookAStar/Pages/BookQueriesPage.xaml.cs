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
            BindingContext = App.CurrentApp.DataManager.BookQueries;
            list.ItemsSource = BindingContext as ObservableCollection<BookQueryData>;
            list.RefreshCommand = BindingContext as ICommand;
            list.IsPullToRefreshEnabled = true;
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
            if (!Queries.IsExecuting)
            Queries.Execute(null);
        }

        private void OnViewDetail(object sender, ItemTappedEventArgs e)
        {
            BookQueryData data = e.Item as BookQueryData;
            App.CurrentApp.ShowBookQuery(data);
            throw new NotImplementedException();
        }
    }
}
