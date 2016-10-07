using BookAStar.Model;
using BookAStar.ViewModels;
using Xamarin.Forms;
using XLabs.Ioc;
using XLabs.Platform.Services;

namespace BookAStar.Pages
{

    public partial class BookQueriesPage : ContentPage
    {
        public BookQueriesPage()
        {
            InitializeComponent();
            BindingContext = new BookQueriesViewModel();
        }

        private void OnViewDetail(object sender, ItemTappedEventArgs e)
        {
            BookQueryData data = e.Item as BookQueryData;
            App.NavigationService.NavigateTo<BookQueryPage>(true,data);
        }
    }
}
