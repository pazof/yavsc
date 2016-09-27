using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookAStar.Helpers
{
    using Model;
    using Model.Blog;
    using Model.Workflow;

    public class DataManager
    {
        // TODO userinfo estimatetemplate rating service product tag   
        public RemoteEntityRO<BookQueryData, long> BookQueries { get; set; }
        public RemoteEntity<Estimate, long> Estimates { get; set; }
        public RemoteEntity<Blog, long> Blogspot { get; set; }
        public LocalEntity<ClientProviderInfo,string> Contacts { get; set; }
        
        public static DataManager Current
        {
            get
            {
                return App.CurrentApp.DataManager;
            }
        }

        public DataManager()
        {
            BookQueries = new RemoteEntityRO<BookQueryData, long>("bookquery",
                q => q.Id);
            Estimates = new RemoteEntity<Estimate, long>("estimate",
                x => x.Id);
            Blogspot = new RemoteEntity<Blog, long>("blog", 
                x=>x.Id);
            Contacts = new LocalEntity<ClientProviderInfo, string>(c => c.UserId);
        }

        public async Task<BookQueryData> GetBookQuery(long bookQueryId)
        {
            return await BookQueries.Get(bookQueryId);
        }
    }
}
