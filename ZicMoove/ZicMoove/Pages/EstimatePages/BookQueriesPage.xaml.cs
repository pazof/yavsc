using ZicMoove.Model;
using ZicMoove.ViewModels;
using Xamarin.Forms;
using XLabs.Ioc;
using XLabs.Platform.Services;

namespace ZicMoove.Pages
{
    using Data;
    using ViewModels.EstimateAndBilling;

    public partial class BookQueriesPage : ContentPage
    {
        public BookQueriesPage()
        {
            InitializeComponent();
            BindingContext = new BookQueriesViewModel();
        }

        public BookQueriesPage(BookQueriesViewModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }

        protected override void OnBindingContextChanged()
        {
            BookQueriesViewModel model = (BookQueriesViewModel) BindingContext;
            if (model!=null)
            {
                model.RefreshQueries =
                    new Command(() =>
                    {
                        DataManager.Instance.BookQueries.Execute(null);
                        this.list.EndRefresh();
                    });
            }
            base.OnBindingContextChanged();
        }

        private void OnViewDetail(object sender, ItemTappedEventArgs e)
        {
            var item = e.Item as BookQueryViewModel;
            App.NavigationService.NavigateTo<BookQueryPage>(true,item);
        }
    }
}
