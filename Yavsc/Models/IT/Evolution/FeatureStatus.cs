namespace Yavsc.Models.IT.Maintaining
{
    /// <summary>
    /// A Feature status
    /// <c>Ko</c>: A Bug has just been discovered
    /// <c>InSane</c>: This feature is not correctly integrating its ecosystem
    /// <c>Obsolete</c>: This will be replaced in a short future, or yet has been replaced
    /// with a better solution.
    /// <c>Ok</c> : nothing to say
    /// </summary>
    public enum FeatureStatus: int
    {
        Requested,
        Accepted,
        Implemented
    }
}