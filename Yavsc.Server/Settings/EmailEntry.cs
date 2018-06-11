using Yavsc.Models.Relationship;

namespace  Yavsc
{
    public class Contact
    {
        public string Name { get; set; }
        public string EMail { get; set; }

        public PostalAddress PostalAddress { get; set; }

    }

}
