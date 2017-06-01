namespace Yavsc.Models.Relationship
{
    public class Relation<TDescription>
    {
        public RelationKind Kind { get; set; }

        TDescription Description { get; set; }

    }
}
