namespace ZicMoove.Data
{
    using Model;
    using Model.Blog;
    using Model.Workflow;
    using Model.Social.Messaging;
    using ViewModels.EstimateAndBilling;
    using NonCrUD;
    using ViewModels;
    using Model.Access;
    using ViewModels.Messaging;
    using Model.Social;

    public class DataManager
    {
        // TODO estimatetemplate rating service product tag   
        public RemoteEntityRO<BookQuery, long> BookQueries { get; set; }
        public RemoteEntityRO<Activity, string> Activities { get; set; }
        public ChatUserCollection ChatUsers { get; set; }
        public EstimateEntity Estimates { get; set; }
        public RemoteEntity<Blog, long> Blogspot { get; set; }
        internal RemoteFilesEntity RemoteFiles { get; set; }

        public LocalEntity<ClientProviderInfo, string> Contacts { get; set; }
        internal RemoteEntity<BlackListed, long> BlackList { get; set; }
        /// <summary>
        /// They've got no remote existence ...
        /// </summary>
        internal LocalEntity<EditEstimateViewModel, long> EstimationCache { get; set; }
        internal LocalEntity<BillingLine, string> EstimateLinesTemplates { get; set; }
        internal LocalEntity<ChatMessage, int> PrivateMessages { get; set; }
        internal LocalEntity<PageState, int> AppState { get; set; }
        internal LocalEntity<string,string> ClientSignatures { get; set; }
        internal LocalEntity<string, string> ProviderSignatures { get; set; }

        protected static DataManager instance = new DataManager();

        public static DataManager Instance 
        {
            get
            {
                return instance;
            }
        }

        public DataManager()
        {
            BookQueries = new RemoteEntityRO<BookQuery, long>("bookquery", q => q.Id);
            Estimates = new EstimateEntity();
            Blogspot = new RemoteEntity<Blog, long>("blog", x=>x.Id);
            Contacts = new LocalEntity<ClientProviderInfo, string>(c => c.UserId);
            AppState = new LocalEntity<PageState, int>(s => s.Position);
            EstimationCache = new LocalEntity<EditEstimateViewModel, long>(e => e.Query.Id);
            EstimateLinesTemplates = new LocalEntity<BillingLine, string>(l => l.Description);
            PrivateMessages = new LocalEntity<ChatMessage, int>(m=> m.GetHashCode());
            RemoteFiles = new RemoteFilesEntity ();
            BlackList = new RemoteEntity<BlackListed, long>("blacklist",u => u.Id);
            ChatUsers = new ChatUserCollection();
            Activities = new RemoteEntityRO<Activity,string>("activity",a=>a.Code);
            PrivateMessages.Load();
            BookQueries.Load();
            Estimates.Load();
            Blogspot.Load();
            Contacts.Load();
            AppState.Load();
            EstimationCache.Load();
            EstimateLinesTemplates.Load();
            RemoteFiles.Load();
            BlackList.Load();
            ChatUsers.Load();
            Activities.Load();
        }
    }
}
