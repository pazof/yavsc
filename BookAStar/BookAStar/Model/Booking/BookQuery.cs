
using System;

namespace BookAStar.Model.Social
{
    public class BookQuery 
    {
        public ClientProviderInfo Client { get; set; }
        public Location Location { get; set; }
        public long Id { get; set; }
        public DateTime EventDate { get; set; }
        public decimal? Previsionnal { get; set; }
        public string Reason { get; set; }
        public bool Read { get; set; }
    }
}
