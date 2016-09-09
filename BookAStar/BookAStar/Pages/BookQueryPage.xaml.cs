
using System;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace BookAStar
{
    using Model;
    using System.Threading.Tasks;

    public partial class BookQueryPage : ContentPage
    {
        private Map map;
        public BookQueryPage(long bookQueryId)
        {
            
            InitializeComponent();
            if (bookQueryId == 0)
                throw new InvalidOperationException("No id");
            BookQueryData bquery = null;
            Task.Run( async () => { bquery = await App.CurrentApp.DataManager.BookQueries.Get(bookQueryId); });
            
            BindingContext = bquery;
            if (bquery==null)
                throw new InvalidOperationException("No data");

            var lat = bquery.Address.Latitude;
            var lon = bquery.Address.Longitude;
            var pin = new Pin
            {
                Type = PinType.SearchResult,
                Position = new Position(
                    lat   , lon ),
                Label = bquery.Title,
                Address = bquery.Address.Address
            };
            pin.BindingContext = bquery;
            map.Pins.Add(pin);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(lat, lon), Distance.FromMeters(100)));
            bookQueryLayout.Children.Add(map);
        }

        private void MakeAnEstimate(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

        }
    }
}
