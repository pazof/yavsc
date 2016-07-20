using BookAStar.Model.Social;
using System.Collections.ObjectModel;

namespace BookAStar
{
    public static class Manager
	{
	    static Manager ()
		{
		}



        // TODO WIP TEST rop this
		private static Location[] _places = new Location[] {
			new Location(48.8626458, 2.2065559, "2 bd Aristide Briand - Suresnes" ),
			new Location(48.8632757, 2.2023099, "Théatre Jean Villard - Suresnes" ),
			new Location(48.8647120, 2.2054588, "Place de la Paix - Suresnes" ),
			new Location(48.8640133, 2.2056573, "Restaurant" ),
			new Location(48.8634839, 2.2064137, "Square" ),
			new Location(48.8653649, 2.2014945, "Stade de foot" ),
		};
        // TODO WIP TEST rop this
        private static ObservableCollection<LocalizedEvent> _your_events = new ObservableCollection<LocalizedEvent> {
            new LocalizedEvent {

                Title = "Yavsc Party",
                Description = "Lancement en fanfare de la version 1.0 de BookAStar, l'appli des fétards",
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

