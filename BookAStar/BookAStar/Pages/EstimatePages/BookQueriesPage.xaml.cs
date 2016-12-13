using BookAStar.Model;
using BookAStar.ViewModels;
using Xamarin.Forms;
using XLabs.Ioc;
using XLabs.Platform.Services;

namespace BookAStar.Pages
{
    using Data;
    using ViewModels.EstimateAndBilling;

    public partial class BookQueriesPage : ContentPage
    {
        public BookQueriesPage()
        {
            InitializeComponent();
            var model = new BookQueriesViewModel();
            model.RefreshQueries =
                new Command( () => {
                    DataManager.Instance.BookQueries.Execute(null);
                    this.list.EndRefresh();
                });
            
            BindingContext = model;
        }

        private void OnViewDetail(object sender, ItemTappedEventArgs e)
        {
            var item = e.Item as BookQueryViewModel;
            App.NavigationService.NavigateTo<BookQueryPage>(true,item);
        }
    }
}
