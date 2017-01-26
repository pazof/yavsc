using Xamarin.Forms;

namespace BookAStar.Pages
{
    using Data;
    using ViewModels;
    using ViewModels.EstimateAndBilling;
    public partial class HomePage 
    {
        public HomePage()
        {
            InitializeComponent();
        }

        public HomePage(HomeViewModel model)
        {
            BindingContext = model;
        }

        public HomeViewModel Model {
            get {
                return (HomeViewModel) BindingContext;
            }
            set
            {
                BindingContext = value;
            }
        }

        protected override void OnBindingContextChanged()
        {
            // this technique make this view model
            // non-sharable between view or pages
            if (Model != null)
            {
                // set the refresh command before using it
                Model.BookQueries.RefreshQueries =
                    new Command(() =>
                    {
                        DataManager.Instance.BookQueries.Execute(null);
                        this.querylist.EndRefresh();
                    });
            }
            // Use the new refresh command
            base.OnBindingContextChanged();

        }

        private void OnViewBookQueryDetail(object sender, ItemTappedEventArgs e)
        {
            var item = e.Item as BookQueryViewModel;
            App.NavigationService.NavigateTo<BookQueryPage>(true, item);
        }
    }
}
