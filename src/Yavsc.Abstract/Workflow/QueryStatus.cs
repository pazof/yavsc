namespace Yavsc
{
    /// <summary>
    /// Status,
     /// should be associated to any
    /// client user query to a provider user or
    /// other external entity.
    /// </summary>
    public enum QueryStatus: int
    {
        Inserted,
        Rejected,
        Accepted,
        InProgress,
        // final states
        Failed,
        Success
    }
}
