using BookAStar.Model.Social;
using System;

namespace BookAStar.Model
{
    public class BookQueryData
    {
        public string Title { get; set; }
        public string Description { set; get; }
        public string Comment { get; set; }
        public long CommandId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Location Address { get; set; }
    }
}
