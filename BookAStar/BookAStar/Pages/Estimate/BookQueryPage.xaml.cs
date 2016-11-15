using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace BookAStar.Pages
{
    using Data;
    using Model;
    using Model.Workflow;
    using System.Linq;
    using ViewModels.EstimateAndBilling;

    public partial class BookQueryPage : ContentPage
    {

        public BookQueryData BookQuery
        {
            get
            {
                return (BindingContext as BookQueryViewModel).Data;
            }
        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            map.Pins.Clear();
            if (BookQuery != null)
            {
                var lat = BookQuery.Location.Latitude;
                var lon = BookQuery.Location.Longitude;
                var pin = new Pin
                {
                    Type = PinType.SavedPin,
                    Position = new Position(
                        lat, lon),
                    Label = BookQuery.Client.UserName,
                    Address = BookQuery.Location.Address
                };
                map.Pins.Add(pin);
                map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Position(lat, lon), Distance.FromMeters(100)));
            }
            
        }
        public BookQueryPage()
        {
            InitializeComponent();
        }
        public BookQueryPage(BookQueryData bookQuery=null)
        {
            
            InitializeComponent();

            // Task.Run( async () => { bookQuery = await App.CurrentApp.DataManager.BookQueries.Get(bookQueryId); });

            BindingContext = new BookQueryViewModel(bookQuery);
        }

        private void OnEditEstimate(object sender, EventArgs ev)
        {
            var bookQueryViewModel = (BookQueryViewModel) BindingContext;

            var editEstimateViewModel = bookQueryViewModel.DraftEstimate;
            if (editEstimateViewModel == null)
            {
                // First search for an existing estimate
                var estimateToEdit = DataManager.Current.Estimates.FirstOrDefault(
                    estimate=> estimate.CommandId == bookQueryViewModel.Id
                    );
                if (estimateToEdit == null)
                {
                    DataManager.Current.Contacts.Merge(BookQuery.Client);
                    DataManager.Current.Contacts.SaveEntity();
                    estimateToEdit = new Estimate()
                    {
                        ClientId = BookQuery.Client.UserId,
                        CommandId = BookQuery.Id,
                        OwnerId = MainSettings.CurrentUser.Id,
                        Id = 0,
                        Description = "# **Hello Estimate!**"
                    };
                    editEstimateViewModel = new EditEstimateViewModel(estimateToEdit, LocalState.New);
                }
                else
                    editEstimateViewModel = new EditEstimateViewModel(estimateToEdit, LocalState.UpToDate);

                DataManager.Current.EstimationCache.Add(editEstimateViewModel);
            }
            App.NavigationService.NavigateTo<EditEstimatePage>(true,
             editEstimateViewModel);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            if (width > height)
            {
                bookQueryLayout.Orientation = StackOrientation.Horizontal;
            }
            else
            {
                bookQueryLayout.Orientation = StackOrientation.Vertical;
            }
            base.OnSizeAllocated(width, height);
        }
    }
}
