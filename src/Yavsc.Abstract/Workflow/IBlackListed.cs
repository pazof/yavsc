namespace Yavsc.Models
{
    public interface IBlackListed
    {
         long Id { get; set; }
         string UserId { get; set; }
         string OwnerId { get; set; }
    }
}