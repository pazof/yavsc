namespace Yavsc.Abstract.Manage
{
    public class EmailSentViewModel
    {
        public string EMail { get; set; }
        public string MessageId { get; set; }
        public bool Sent { get; set; }
        public string ErrorMessage { get; set; }
    }
}