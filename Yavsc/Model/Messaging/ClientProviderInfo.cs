
using System;
namespace Yavsc.Model
{

public class BookQueryProviderView { 
            public ClientProviderView Client { get; set; }
            public Location Location { get; set; }

            public long Id { get; set; }

            public DateTime EventDate { get ; set; }
            public decimal? Previsional { get; set; }
        }
        public class ClientProviderView { 
            public string UserName { get; set; }
            public string UserId { get; set; }
            public int Rate { get; set; }
        }
        
}