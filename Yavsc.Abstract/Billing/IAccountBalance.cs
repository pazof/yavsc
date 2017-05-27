namespace Yavsc
{
    public interface IAccountBalance
    {
        long ContactCredits { get; set; }
        decimal Credits { get; set; }
        string UserId { get; set; }
    }
}
