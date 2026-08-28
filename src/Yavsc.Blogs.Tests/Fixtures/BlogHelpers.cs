namespace Yavsc.Blogs.Tests.Fixtures;


public static class BlogHelpers
{
    public static string BlogUrl(this IBackendFixture fixture)
        => $"{fixture.Addresses.First(a => a.StartsWith("https://"))}/{Constants.APIPrefix}/{Constants.BlogSpotPath}";

    public static string BlogAclUrl(this IBackendFixture fixture)
        => $"{fixture.Addresses.First(a => a.StartsWith("https://"))}/{Constants.APIPrefix}/{Constants.BlogAclPath}";
}