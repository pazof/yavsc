using System;
using Yavsc.Models;

namespace BookAStar.Model.Blog
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

        public DateTime Modified
        {
            get; set;
        }

        public string Photo
        {
            get; set;
        }

        public DateTime Posted
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
