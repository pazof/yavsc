public interface IBackendFixture : IDisposable
{
    /// <summary>
    /// The addresses the fixture bound to.
    /// </summary>
    IReadOnlyList<string> Addresses { get; }

    /// <summary>
    /// The service provider for the fixture host.
    /// </summary>
    IServiceProvider Services { get; }

}
