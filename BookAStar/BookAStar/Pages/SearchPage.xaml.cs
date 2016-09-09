using System;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using BookAStar.Model.Social;
using BookAStar.Model.Workflow.Messaging;

namespace BookAStar
{
    public partial class SearchPage : ContentPage
	{
		public ObservableCollection<LocalizedEvent> Events { get; private set; }
		public SearchPage ()
		{

			InitializeComponent ();

			BindingContext = this;
			Events = Manager.Events;
			list.ItemTapped += async (object sender, ItemTappedEventArgs e) => {
				await Navigation.PushAsync(new EventDetail( (YaEvent) e.Item) { Title = "Détail de la soirée" } );
			};
			search_date.Date = DateTime.Now;
			search_date.MinimumDate = DateTime.Now;
			search_phrase.Text = "Suresnes";
			btn_update.Clicked += (object sender, EventArgs e) => {
				// 
			};
		}
	}
}
