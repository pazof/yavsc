using ZicMoove.Model.Social;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Yavsc;

namespace ZicMoove.Pages
{
	public class PinPage : ContentPage
	{
		Map map;
		/*
		protected async Task<Geolocator.Plugin.Abstractions.Position> GetPos() {
			var locator = CrossGeolocator.Current;
			locator.DesiredAccuracy = 50;

			return await locator.GetPositionAsync (timeout: 10000);
		}

		
		private Position _pos;
		public Position MyPosition {
			get {
				return _pos;
			}
		}
	
		public async Task UpdateMyPos()
		{
			var pos = await GetPos();
			_pos = new Position(pos.Latitude,pos.Longitude);
		}
*/
		protected override void OnAppearing ()
		{
			base.OnAppearing ();
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
			double lat=0;
			double lon=0;
			int pc = 0;
			foreach (LocalizedEvent ev in Manager.Events) {
				var pin = new Pin {
					Type = PinType.SearchResult,
					Position = new Xamarin.Forms.Maps.Position(
						ev.Location.Latitude, ev.Location.Longitude),
					Label = ev.Title,
					Address = ev.Location.Address
				};
				pin.BindingContext = ev;
				map.Pins.Add (pin);
                // TODO find a true solution
				lat = (lat * pc + ev.Location.Latitude) / (pc + 1);
				lon = (lon * pc + ev.Location.Longitude) / (pc + 1);
				pc++;
			}
			// TODO build a MapSpan covering events
			map.MoveToRegion(MapSpan.FromCenterAndRadius (
				new Xamarin.Forms.Maps.Position(lat,lon), Distance.FromMeters(100)));
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
			var street = new Button { Text = "Street" };
			var hybrid = new Button { Text = "Hybrid" };
			var satellite = new Button { Text = "Satellite" };

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

		void HandleMapStyleClicked (object sender, EventArgs e)
		{
			var b = sender as Button;
			switch (b.Text) {
			case "Street":
				map.MapType = MapType.Street;
				break;
			case "Hybrid":
				map.MapType = MapType.Hybrid;
				break;
			case "Satellite":
				map.MapType = MapType.Satellite;
				break;
			}
		}
	}
}

