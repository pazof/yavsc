namespace YavscLib
{
    public interface ISpecializationSettings
    {
        string UserId { get ; set ; }
        bool ExistsInDb(object dbContext);
    }
}