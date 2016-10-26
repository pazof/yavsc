namespace BookAStar.Data
{
    using Model;
    using Model.Blog;
    using Model.Workflow;
    using Model.UI;
    using ViewModels;
    using Model.Social.Messaging;

    public class DataManager
    {
        // TODO estimatetemplate rating service product tag   
        public RemoteEntityRO<BookQueryData, long> BookQueries { get; set; }
        public RemoteEntity<Estimate, long> Estimates { get; set; }
        public RemoteEntity<Blog, long> Blogspot { get; set; }
        public LocalEntity<ClientProviderInfo,string> Contacts { get; set; }
        internal LocalEntity<PageState, int> AppState { get; set; }
        /// <summary>
        /// They have no remote exisence ...
        /// </summary>
        internal LocalEntity<EditEstimateViewModel, long> EstimationCache { get; set; }
        internal LocalEntity<BillingLine, string> EstimateLinesTemplates { get; set; }
        internal LocalEntity<UserMessage, int> PrivateMessages { get; set; }
        protected static DataManager current ;

        public static DataManager Current 
        {
            get
            {
                if (current == null)
                    current = new DataManager();
                return current;
            }
        }

        public DataManager()
        {
            BookQueries = new RemoteEntityRO<BookQueryData, long>("bookquery", q => q.Id);
            Estimates = new RemoteEntity<Estimate, long>("estimate", x => x.Id);
            Blogspot = new RemoteEntity<Blog, long>("blog", x=>x.Id);
            Contacts = new LocalEntity<ClientProviderInfo, string>(c => c.UserId);
            AppState = new LocalEntity<PageState, int>(s => s.Position);
            EstimationCache = new LocalEntity<EditEstimateViewModel, long>(e => e.Query.Id);
            EstimateLinesTemplates = new LocalEntity<BillingLine, string>(l => l.Description);
            PrivateMessages = new LocalEntity<UserMessage, int>(m=> m.GetHashCode());

            PrivateMessages.Load();
            BookQueries.Load();
            Estimates.Load();
            Blogspot.Load();
            Contacts.Load();
            AppState.Load();
            EstimationCache.Load();
            EstimateLinesTemplates.Load();
        }
    }
}
