﻿using ZicMoove.Model.Social;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Yavsc;
using ZicMoove.Data;
using System.Threading.Tasks;
using XLabs.Platform.Services.Geolocation;
using Plugin.Geolocator;

namespace ZicMoove.Pages
{
	public class PinPage : ContentPage
	{
		protected Map map;
        
		protected async Task<Plugin.Geolocator.Abstractions.Position> GetPos() {
			var locator = CrossGeolocator.Current;
			locator.DesiredAccuracy = 50;

			return await locator.GetPositionAsync (10000);
		}

		
		private Model.Social.Position _pos;
		public Model.Social.Position MyPosition {
			get {
				return _pos;
			}
		}
	
		public async Task UpdateMyPos()
		{
			var pos = await GetPos();
			_pos = new Model.Social.Position(pos.Latitude,pos.Longitude);
		}

		protected override void OnAppearing ()
		{
			base.OnAppearing ();
		}
        protected virtual void OnInitMap ()
        {
            double lat = 0;
            double lon = 0;
            int pc = 0;
            foreach (var query in DataManager.Instance.BookQueries)
            {
                var pin = new Pin
                {
                    Type = PinType.Generic,
                    Position = new Xamarin.Forms.Maps.Position(
                        query.Location.Latitude, query.Location.Longitude),
                    Label = query.Reason,
                    Address = query.Location.Address
                };
                pin.BindingContext = query;
                map.Pins.Add(pin);
                // TODO find a true solution :-/
                lat = (lat * pc + query.Location.Latitude) / (pc + 1);
                lon = (lon * pc + query.Location.Longitude) / (pc + 1);
                pc++;
            }
            // TODO build a MapSpan covering events
            map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Xamarin.Forms.Maps.Position(lat, lon), Distance.FromMeters(100)));

        }

        public PinPage ()
		{

			map = new Map { 
				IsShowingUser = true,
				HeightRequest = 100,
				WidthRequest = 960,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand
			};
            OnInitMap();
            /*
			// A "relocate" button : useless, since it yet exists

			var reLocate = new Button { Text = "Re-centrer" };
			reLocate.Clicked += async delegate {
				await UpdateMyPos();
				Debug.WriteLine("Position: {0} {1}",MyPosition.Longitude,MyPosition.Latitude);
				map.MoveToRegion (MapSpan.FromCenterAndRadius (
					MyPosition, Distance.FromMeters (100)));
			};
			var buttons = new StackLayout {
				Orientation = StackOrientation.Horizontal,
				Children = {
					reLocate
				}
			};
			*/
            // create map style buttons
            var street = new Button { Text = Strings.Street };
			var hybrid = new Button { Text = Strings.Hybrid };
			var satellite = new Button { Text = Strings.Satellite };

			street.Clicked += HandleMapStyleClicked;
			hybrid.Clicked += HandleMapStyleClicked;
			satellite.Clicked += HandleMapStyleClicked;
			var segments = new StackLayout { Spacing = 30,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				Orientation = StackOrientation.Horizontal, 
				Children = {street, hybrid, satellite}
			};

			// put the page together
			Content = new StackLayout { 
				Spacing = 0,
				Children = {
					map,
					segments
				}};
		}
        protected StackLayout StackLayout
        { get
            {
                return Content as StackLayout;
            } }

        void HandleMapStyleClicked (object sender, EventArgs e)
		{
			var b = sender as Button;
			if (b.Text== Strings.Street)
				map.MapType = MapType.Street;
            else if (b.Text == Strings.Hybrid)
                map.MapType = MapType.Hybrid;
            else if (b.Text == Strings.Satellite)
                map.MapType = MapType.Satellite;
		}
	}
}

