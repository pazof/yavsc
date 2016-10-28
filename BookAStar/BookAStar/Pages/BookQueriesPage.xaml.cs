using BookAStar.Model;
using BookAStar.ViewModels;
using Xamarin.Forms;
using XLabs.Ioc;
using XLabs.Platform.Services;

namespace BookAStar.Pages
{
    using Data;
    public partial class BookQueriesPage : ContentPage
    {
        public BookQueriesPage()
        {
            InitializeComponent();
            var model = new BookQueriesViewModel();
            model.RefreshQueries =
                new Command( () => {
                    DataManager.Current.BookQueries.Execute(null);
                    this.list.EndRefresh();
                });
            
            BindingContext = model;
        }

        private void OnViewDetail(object sender, ItemTappedEventArgs e)
        {
            BookQueryData data = e.Item as BookQueryData;
            App.NavigationService.NavigateTo<BookQueryPage>(true,data);
        }
    }
}
