namespace Yavsc.Tests.Shared;

/// <summary>
/// Helpers for the in-memory connection string used by test fixtures.
///
/// EF Core's <c>UseInMemoryDatabase(name)</c> returns the same backing
/// store to every <c>DbContext</c> that asks for it under the same
/// <paramref name="name"/>, in the same process. That means every
/// fixture that uses the bare <c>"InMemory"</c> connection string
/// shares the same in-memory database — which leaks state between
/// fixtures that are supposed to be independent, and silently makes
/// tests order-dependent.
///
/// The fix is to give each fixture its own suffix. <see cref="For"/>
/// returns a stable, fixture-scoped connection string. The fixture
/// stores the suffix in an instance field so successive calls within
/// the same fixture always resolve to the same database.
/// </summary>
public static class InMemoryDatabaseName
{
    /// <summary>Base connection string for the in-memory provider,
    /// as it appears in <c>appsettings-org.Testing.json</c>.</summary>
    public const string Base = "InMemory";

    /// <summary>Builds a per-fixture connection string. Two calls
    /// with the same <paramref name="fixtureId"/> return the same
    /// string; two calls with different ids return different
    /// strings, isolating the underlying in-memory stores.</summary>
    public static string For(string fixtureId) => $"{Base}-{fixtureId}";
}
