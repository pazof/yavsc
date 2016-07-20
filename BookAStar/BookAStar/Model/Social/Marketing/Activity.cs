namespace BookAStar.Model.Workflow.Marketing
{
    public class Activity
    {
        
        public string Code {get; set;}
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string Name {get; set;}

        public string Description {get; set;}
        /// <summary>
        /// Name to associate to a performer in this activity domain
        /// </summary>
        /// <returns></returns>
        public string ActorDenomination {get; set;}

        public string Photo {get; set;}
        
        
        /// <summary>
        /// Moderation settings
        /// </summary>
        /// <returns></returns>
        string ModeratorGroupName { get; set; }

        

    }
}
