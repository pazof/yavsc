using System;
using YavscLib;

namespace ZicMoove.Model.Blog
{
    public partial class Blog : IBlog
    {
        public string AuthorId
        {
            get; set;
        }

        public string Content
        {
            get; set;
        }

        public long Id
        {
            get; set;
        }

        public DateTime DateModified
        {
            get; set;
        }

        public string UserCreated { get; set; }

        public string UserModified { get; set; }

        public string Photo
        {
            get; set;
        }

        public DateTime DateCreated
        {
            get; set;
        }

        public int Rate
        {
            get; set;
        }

        public string Title
        {
            get; set;
        }

        public bool Visible
        {
            get; set;
        }
    }
}
