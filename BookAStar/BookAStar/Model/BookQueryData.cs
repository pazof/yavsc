using BookAStar.Model.Social;
using System;

namespace BookAStar.Model
{
    public class ClientProviderInfo
    {
        public string UserName { get; set; }
        public string UserId { get; set;  }
        public int Rate { get; set; }
    }
    public class BookQueryData
    {
        public ClientProviderInfo Client { get; set; }
        public Location Location { get; set; }
        public long Id { get; set; }
        public DateTime EventDate { get; set; }
        public decimal? Previsionnal { get; set; }
    }
}
