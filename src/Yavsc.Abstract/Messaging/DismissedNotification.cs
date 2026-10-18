namespace Yavsc.Abstract.Models.Messaging
{
    /// <summary>
    /// Wire format of a notification dismissed by the current user.
    /// The user is always implied by the authenticated caller.
    /// </summary>
    public sealed class DismissedNotification
    {
        public long NotificationId { get; set; }
    }
}
