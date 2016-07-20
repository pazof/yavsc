

namespace BookAStar.Model.Workflow.Messaging
{
    /// <summary>
    /// A Notification, that mocks the one sent to Google,
    /// since it fits my needs
    /// </summary>
    public class Notification
    {
        public long Id { get; set; }
        /// <summary>
        /// The title.
        /// </summary>
        public string title { get; set; }
       /// <summary>
        /// The body.
        /// </summary>
        public string body { get; set; }
        /// <summary>
        /// The icon.
        /// </summary>
        public string icon { get; set; }
        /// <summary>
        /// The sound.
        /// </summary>
        public string sound { get; set; }
        /// <summary>
        /// The tag.
        /// </summary>
        public string tag { get; set; }
        /// <summary>
        /// The color.
        /// </summary>
        public string color { get; set; }
        /// <summary>
        /// The click action.
        /// </summary>
        public string click_action { get; set; }
    }
}