namespace Yavsc.ViewModels.Blogspot
{
    public class BlogIndexKey
    {
        public string AuthorId { get; set; }
        public string Title { get; set; }
         // override object.Equals
        public override bool Equals (object obj)
        {
            //
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var blogindexkey = (BlogIndexKey)obj;
            return Title == blogindexkey.Title && AuthorId == blogindexkey.AuthorId;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Title.GetHashCode() * AuthorId.GetHashCode();
        }
    }
}
