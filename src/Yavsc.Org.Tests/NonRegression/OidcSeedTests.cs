using Xunit;

namespace Yavsc
{
    /// <summary>
    /// Non-regression tests for the IdentityServer8 configuration seed.
    /// Today (2026-06-25) we hit a runtime crash on prod because the
    /// seed attempted to insert 'openid' / 'profile' both as
    /// IdentityResources and as ApiScopes:
    ///
    ///   Found identity scopes and API scopes that use the same names.
    ///   This is an invalid configuration. Use different names for
    ///   identity scopes and API scopes. Scopes found: openid, profile
    ///
    /// These tests pin the invariants that prevent that mistake from
    /// regressing: the application-defined scope list must never
    /// contain names that collide with identity scopes, every entry
    /// must reference an ApiResource, and the seed's translation to
    /// SQL must stay valid.
    /// </summary>
    [Trait("regression", "OIDC seed")]
    public class OidcSeedTests
    {
        // Identity scopes that IdentityServer8 manages on its own. They
        // must never appear in Constants.BuildInApiScopes nor in
        // Constants.ApiResourcesScopes — an ApiScope that shares its
        // Name with an IdentityResource prevents the host from starting.
        private static readonly string[] ReservedIdentityScopeNames =
        {
            "openid",
            "profile",
            "offline_access"
        };

        [Fact]
        public void BuildInApiScopes_DoesNotContainIdentityScopes()
        {
            foreach (var reserved in ReservedIdentityScopeNames)
            {
                Assert.DoesNotContain(reserved, Constants.BuildInApiScopes);
            }
        }

        [Fact]
        public void ApiResourcesScopes_DoesNotContainIdentityScopes()
        {
            foreach (var reserved in ReservedIdentityScopeNames)
            {
                foreach (var spec in Constants.ApiResourcesScopes)
                {
                    Assert.NotEqual(reserved, spec.ScopeName);
                }
            }
        }

        [Fact]
        public void ApiResourcesScopes_EveryEntryReferencesAUniqueResource()
        {
            // Same ScopeName appearing twice would mean two ApiScopes
            // (and two ApiResourceScopes rows) trying to claim the same
            // name — IdentityServer8 would reject that on startup.
            var scopeNames = Constants.ApiResourcesScopes
                .Select(s => s.ScopeName)
                .ToList();
            Assert.Equal(scopeNames.Count, scopeNames.Distinct().Count());

            // Same ResourceName appearing twice would mean two
            // ApiResources rows with the same Name — the EF seed would
            // crash on the unique index.
            var resourceNames = Constants.ApiResourcesScopes
                .Select(s => s.ResourceName)
                .ToList();
            Assert.Equal(resourceNames.Count, resourceNames.Distinct().Count());
        }

        [Fact]
        public void ApiResourcesScopes_EveryEntryHasNonEmptyStrings()
        {
            // An empty ScopeName or ResourceName silently produces an
            // ApiScope / ApiResource row with no key, which breaks the
            // scope validator's string-based lookup later on.
            foreach (var spec in Constants.ApiResourcesScopes)
            {
                Assert.False(string.IsNullOrWhiteSpace(spec.ScopeName),
                    $"ScopeName must be set (got '{spec.ScopeName}')");
                Assert.False(string.IsNullOrWhiteSpace(spec.ResourceName),
                    $"ResourceName must be set (got '{spec.ResourceName}')");
                Assert.False(string.IsNullOrWhiteSpace(spec.ResourceDisplayName),
                    $"ResourceDisplayName must be set (got '{spec.ResourceDisplayName}')");
            }
        }

        [Fact]
        public void ApiResourcesScopes_IsNotEmpty()
        {
            // Belt and braces: if someone ever wipes the list thinking
            // it's stale, this test forces them to be explicit about it.
            Assert.NotEmpty(Constants.ApiResourcesScopes);
        }
    }
}