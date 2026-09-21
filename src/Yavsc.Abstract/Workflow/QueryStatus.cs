namespace Yavsc
{
    /// <summary>
    /// Status,
     /// should be associated to any
    /// client user query to a provider user or
    /// other external entity.
    ///
    /// <para>
    /// Acceptance is split by party: <see cref="ProAccepted"/> when the
    /// provider accepts the client's request (and produces/submits the
    /// estimate), <see cref="ClientAccepted"/> when the client accepts
    /// the provider's estimate. <see cref="Accepted"/> is the legacy
    /// single-value kept for rows written before the split; new writes
    /// use the party-specific values. Its integer value (2) is stable
    /// so existing rows still deserialize.
    /// </para>
    /// </summary>
    public enum QueryStatus: int
    {
        Inserted = 0,
        Rejected = 1,
        Accepted = 2,
        InProgress = 3,
        // final states
        Failed = 4,
        Success = 5,
        // party-specific acceptance (see summary)
        ProAccepted = 6,
        ClientAccepted = 7,
    }
}