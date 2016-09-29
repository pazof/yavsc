
using System;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace BookAStar.Pages
{
    using Helpers;
    using Model;
    using Model.Workflow;
    using System.Threading.Tasks;
    using XLabs.Forms.Mvvm;
    using XLabs.Ioc;
    using XLabs.Platform.Services;

    public partial class BookQueryPage : ContentPage
    {

        public BookQueryData BookQuery
        {
            get
            {
                return BindingContext as BookQueryData;
            }
            set
            {
                BindingContext = value;
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
        
        public BookQueryPage(BookQueryData bookQuery=null)
        {
            
            InitializeComponent();

            // Task.Run( async () => { bookQuery = await App.CurrentApp.DataManager.BookQueries.Get(bookQueryId); });
            
            BookQuery = bookQuery;
            
        }

        private void MakeAnEstimate(object sender, EventArgs ev)
        {
            DataManager.Current.Contacts.Merge(BookQuery.Client);
            var e = new Estimate()
            {
                ClientId = BookQuery.Client.UserId,
                CommandId = BookQuery.Id,
                OwnerId = MainSettings.CurrentUser.Id,
                Id = 0,
                Description = "# **Hello Estimate!**"
            };
            Resolver.Resolve<INavigationService>().NavigateTo<EditEstimatePage>(true, e);
        }
        
    }
}
