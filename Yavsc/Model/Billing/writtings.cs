
namespace Yavsc.Models.Billing
{
    public partial class writtings
    {
        public long _id { get; set; }
        public int? count { get; set; }
        public string description { get; set; }
        public long estimid { get; set; }
        public string productid { get; set; }
        public decimal? ucost { get; set; }
    }
}
