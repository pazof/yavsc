

using IdentityServer8.EntityFramework.Entities;

public static class Constants
{
     public static readonly string[] BuildInApiScopes = {
        "profile", "openid", "offline_access",
        "admin", "moderation", "performer", "client" };

    // One ApiResource per application scope. Each scope is exposed by
    // exactly one resource, named after the scope ("admin" -> "admin"
    // resource, "blogs" -> "blogs" resource). IdentityServer8's
    // DefaultResourceValidator only recognises a scope at the
    // /connect/authorize endpoint if it can find an ApiResource that
    // exposes it — an orphaned ApiScope row is rejected with
    // "Scope X not found in store" even though the row exists.
    public static readonly ApiResourceScopeSpecification[] ApiResourcesScopes = {

       new ApiResourceScopeSpecification { ScopeName = "admin", Description = "Admin access", ResourceName = "admin", ResourceDisplayName = "Admin API" },
       new ApiResourceScopeSpecification { ScopeName = "moderation", Description = "Moderation access", ResourceName = "moderation", ResourceDisplayName = "Moderation API" },
       new ApiResourceScopeSpecification { ScopeName = "performer", Description = "Performer access", ResourceName = "performer", ResourceDisplayName = "Performer API" },
       new ApiResourceScopeSpecification { ScopeName = "client", Description = "Client access", ResourceName = "client", ResourceDisplayName = "Client API" },
       new ApiResourceScopeSpecification { ScopeName = "blogs", Description = "Blogs access", ResourceName = "blogs", ResourceDisplayName = "Yavsc Blogs API" }
    };
}

public class ApiResourceScopeSpecification
{
    public string ScopeName { get; set; }
    public string Description { get; set; }
    // The ApiResource that exposes this scope. IdentityServer8 requires
    // scopes to be linked back to an ApiResource via the ApiResourceScopes
    // table — without that link the scope is considered unknown.
    public string ResourceName { get; set; }
    public string ResourceDisplayName { get; set; }
}
