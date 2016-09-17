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
        // TODO userinfo estimatetemplate rating service product tag   
        public RemoteEntity<BookQueryData, long> BookQueries { get; set; }
        public RemoteEntity<Estimate, long> Estimates { get; set; }
        public RemoteEntity<Blog, long> Blogspot { get; set; }
        public static DataManager Current
        {
            get
            {
                return App.CurrentApp.DataManager;
            }
        }
        public DataManager()
        {
            BookQueries = new RemoteEntity<BookQueryData, long>("bookquery",
                q => q.Id);
            Estimates = new RemoteEntity<Estimate, long>("estimate",
                x => x.Id);
            Blogspot = new RemoteEntity<Blog, long>("blog", 
                x=>x.Id);
        }
        public async Task<BookQueryData> GetBookQuery(long bookQueryId)
        {
            return await BookQueries.Get(bookQueryId);
        }
    }
}
