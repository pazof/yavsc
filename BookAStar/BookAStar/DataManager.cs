using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookAStar
{
    using Model;
    using Model.Blog;
    using Model.Workflow;

    public class DataManager
    {
        public RemoteEntity<BookQueryData, long> BookQueries { get; set; }
        public RemoteEntity<Estimate, long> Estimates { get; set; }
        public RemoteEntity<Blog, long> Blogspot { get; set; }

        public DataManager()
        {
            BookQueries = new RemoteEntity<BookQueryData, long>("bookqueries",
                q => q.CommandId);
            Estimates = new RemoteEntity<Estimate, long>("estimates",
                x => x.Id);
            Blogspot = new RemoteEntity<Blog, long>("blogspot", 
                x=>x.Id);
        }
        public async Task<BookQueryData> GetBookQuery(long bookQueryId)
        {
            return await BookQueries.Get(bookQueryId);
        }
    }
}
