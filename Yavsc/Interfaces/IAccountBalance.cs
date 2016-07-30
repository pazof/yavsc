namespace Yavsc.Interfaces
{
    public interface IAccountBalance
    {
        long ContactCredits { get; set; }
        decimal Credits { get; set; }
        string UserId { get; set; }
    }
}