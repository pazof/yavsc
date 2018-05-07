namespace Yavsc.Interfaces
{
    public interface ITaggable<K> 
    {
         string [] GetTags();

         K Id { get; }
    }
}