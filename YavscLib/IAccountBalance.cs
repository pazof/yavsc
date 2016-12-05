namespace Yavsc.Models
{
    public interface IAccountBalance
    {
        long ContactCredits { get; set; }
        decimal Credits { get; set; }
        IApplicationUser Owner { get; set; }
        string UserId { get; set; }
    }
}