using Yavsc.Models.Relationship;

namespace Yavsc.Interfaces
{
    public interface ITaggable<K> 
    {
         void Tag(Tag tag);
         void Detag(Tag tag);

         string [] GetTags();

         K Id { get; }
    }
}