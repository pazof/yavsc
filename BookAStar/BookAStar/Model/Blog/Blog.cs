using System;

namespace BookAStar.Model.Blog
{
    public partial class Blog
    {

        public long Id { get; set; }

        public string bcontent { get; set; }
        
        public DateTime modified { get; set; }

        public string photo { get; set; }

        public DateTime posted { get; set; }

        public int rate { get; set; }

        public string title { get; set; }

        public string AuthorId { get; set; }

        public bool visible { get; set; }
    }
}
