namespace Yavsc.Models.IT.Fixing
{
    /// <summary>
    /// Bug status:
    /// * Inserted -> Confirmed|FeatureRequest|Feature|Rejected
    /// * Confirmed -> Fixed
    /// * FeatureRequest -> Implemented
    /// </summary>
    public enum BugStatus : int
    {
        Inserted,
        Confirmed,
        Rejected,
        Feature,
        Fixed
    }
}