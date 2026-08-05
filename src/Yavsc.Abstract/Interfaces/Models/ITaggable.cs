namespace Yavsc.Interfaces
{
    public interface ITaggable<K> : IIdentified<K>
    {
         string [] GetTags();

         K Id { get; }
    }
}
