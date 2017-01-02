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
            base.OnBindingContextChanged();
            if (Model != null)
            {
                Model.BookQueries.RefreshQueries =
                    new Command(() =>
                    {
                        DataManager.Instance.BookQueries.Execute(null);
                        this.querylist.EndRefresh();
                    });
            }
        }

        private void OnViewBookQueryDetail(object sender, ItemTappedEventArgs e)
        {
            var item = e.Item as BookQueryViewModel;
            App.NavigationService.NavigateTo<BookQueryPage>(true, item);
        }
    }
}
