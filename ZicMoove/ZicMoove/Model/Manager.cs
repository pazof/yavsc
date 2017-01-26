using ZicMoove.Model.Social;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ZicMoove
{      
    [Obsolete("Use Helpers.DataManager")]
    public static class Manager
	{
	    static Manager ()
		{
		}

        public static ICommand RefreshBookQueries;
        // TODO WIP TEST rop this
        private static Location[] _places = new Location[] {
			new Location { Latitude = 48.8626458, Longitude = 2.2065559, Address = "2 bd Aristide Briand - Suresnes" },
			new Location{ Latitude =48.8632757, Longitude =2.2023099, Address ="Théatre Jean Villard - Suresnes" },
			new Location{ Latitude =48.8647120, Longitude =2.2054588,Address = "Place de la Paix - Suresnes" },
			new Location{ Latitude =48.8640133, Longitude =2.2056573, Address ="Restaurant" },
			new Location{ Latitude =48.8634839, Longitude =2.2064137,Address = "Square" },
			new Location{ Latitude =48.8653649, Longitude =2.2014945,Address = "Stade de foot" },
		};
        // TODO WIP TEST rop this
        private static ObservableCollection<LocalizedEvent> _your_events = new ObservableCollection<LocalizedEvent> {
            new LocalizedEvent {

                Title = "Yavsc Party",
                Description = "Lancement en fanfare de la version 1.0 de ZicMoove, l'appli des fétards",
                ProviderId = "paul",
                ProviderName = "Yavsc Fondation",
                EventWebPage = "http://lua.pschneider.fr/",
                Location = _places[0]
			},
			new LocalizedEvent {
				Title = "Evenement de test",
				Description = "Blah bli lo qui est errare, ma no. Blou test allo 3!", 
				ProviderId = "provid3",
				ProviderName = "Prov Entreprise 3",
				EventWebPage = "http://lua.pschneider.fr/events/test3",
                Location = _places[1]
            },
			new LocalizedEvent {
				Title = "DjFx feat XamCoder, en Concert gratuit",
				Description = "Hip Hop à Suresnes",
				ProviderId = "brahim",
				ProviderName = "Totem Production",
				EventWebPage = "http://lua.pschneider.fr/events/totem",
                Location = _places[2]
            }
		};
        // TODO WIP TEST rop this
        public static ObservableCollection<LocalizedEvent> Events {
			get {
				return _your_events;
			}
		}

	}
}

