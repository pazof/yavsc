using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace ZicMoove.Pages
{
    using Data;
    using EstimatePages;
    using Model.Musical;
    using Model.Social;
    using Model.Workflow;
    using Settings;
    using ViewModels.EstimateAndBilling;

    public partial class BookQueryPage : ContentPage
    {

        public BookQuery BookQuery
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
                    Position = new Xamarin.Forms.Maps.Position
                    (lat, lon),
                    Label = BookQuery.Client.UserName,
                    Address = BookQuery.Location.Address
                };
                map.Pins.Add(pin);
                map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Xamarin.Forms.Maps.Position(lat, lon), Distance.FromMeters(100)));
            }
            
        }
        public BookQueryPage()
        {
            InitializeComponent();
        }
        public BookQueryPage(BookQueryViewModel bookQuery =null)
        {
            
            InitializeComponent();
            // when TODO update?
            // Task.Run( async () => { bookQuery = await App.CurrentApp.DataManager.BookQueries.Get(bookQueryId); });

            BindingContext = bookQuery;
        }

        private void OnEditEstimate(object sender, EventArgs ev)
        {
            var bookQueryViewModel = (BookQueryViewModel) BindingContext;

            var editEstimateViewModel = bookQueryViewModel.DraftEstimate;
            if (editEstimateViewModel == null)
            {
                // First search for an existing estimate
                editEstimateViewModel = DataManager.Instance.EstimationCache.LocalGet(
                     bookQueryViewModel.Id
                    );
                if (editEstimateViewModel == null)
                {
                    DataManager.Instance.Contacts.Merge(BookQuery.Client);
                    DataManager.Instance.Contacts.SaveEntity();
                    editEstimateViewModel = new EditEstimateViewModel( new Estimate
                    {
                        ClientId = BookQuery.Client.UserId,
                        CommandId = BookQuery.Id,
                        OwnerId = MainSettings.CurrentUser.Id,
                        Id = 0
                    });
                    DataManager.Instance.EstimationCache.Add(editEstimateViewModel);
                }
            }
            App.NavigationService.NavigateTo<EditEstimatePage>(true,
             editEstimateViewModel);
        }

        private async void OnViewEstimate(object sender, EventArgs ev)
        {
            var bookQueryViewModel = (BookQueryViewModel) BindingContext;
            var buttons = bookQueryViewModel.Estimates.Select(e => $"{e.Id} / {e.Title} / {e.Total}").ToArray();
            var action = await App.DisplayActionSheet("Estimations validées", "Annuler", null, buttons);
            if (buttons.Contains(action))
            {
                var index = Array.IndexOf(buttons,action);
                var estimate = bookQueryViewModel.Estimates[index];
                App.NavigationService.NavigateTo<ViewEstimatePage>(true,
                new EditEstimateViewModel(estimate));
            }
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

        private void OnBlockThisUser(object sender, EventArgs ev)
        {
            var bookQueryViewModel = (BookQueryViewModel)BindingContext;
            DataManager.Instance.BlackList.Add(
                new Model.Access.BlackListed
                {
                    OwnerId = MainSettings.CurrentUser.Id,
                    UserId = bookQueryViewModel.Data.Client.UserId
                });
            DataManager.Instance.BlackList.SaveEntity();
        }

        private void OnDropQuery(object sender, EventArgs ev)
        {
            var bookQueryViewModel = (BookQueryViewModel)BindingContext;
            bookQueryViewModel.Rejected = true;
            DataManager.Instance.EstimationCache.SaveEntity();
        }
    }
}
