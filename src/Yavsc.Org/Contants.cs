

using IdentityServer8.EntityFramework.Entities;

public static class Constants
{
     public static readonly string[] BuildInApiScopes = {
        "profile", "openid", "offline_access",
        "admin", "moderation", "performer", "client" };

    public static readonly ApiResourceScopeSpecification[] ApiResourcesScopes = {

       new ApiResourceScopeSpecification { ScopeName = "admin", Description = "Admin access" },
       new ApiResourceScopeSpecification { ScopeName = "moderation", Description = "Moderation access" },
       new ApiResourceScopeSpecification { ScopeName = "performer", Description = "Performer access" },
       new ApiResourceScopeSpecification { ScopeName = "client", Description = "Client access" },
       new ApiResourceScopeSpecification { ScopeName = "blogs", Description = "Blogs access" }
    };
}

public class ApiResourceScopeSpecification
{
    public string ScopeName { get; set; }
    public string Description { get; set; }
}
