namespace Yavsc
{
    public interface ILocation : IPosition
    {
        string Address { get; set; }
        long Id { get; set; }
    }
}
