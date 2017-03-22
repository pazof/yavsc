namespace YavscLib.HairCut
{
    public interface IHairCutQuery
    {
        long Id { get; set; }
        long PrestationId { get; set; }

        long LocationId { get; set; }
    }
}
