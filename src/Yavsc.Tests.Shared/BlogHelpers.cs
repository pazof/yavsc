
namespace Yavsc.Blogs.Tests.Fixtures;
using static Yavsc.Constants;

public static class BlogHelpers
{
    public static string ApiUrl(this IBackendFixture fixture, string apiSubPath)
    {
        var secured = fixture.Addresses.FirstOrDefault(a => a.StartsWith("https://"));
        if (secured is  null)
        {
            var unsecured = fixture.Addresses.FirstOrDefault(a => a.StartsWith("http://"));
            if (unsecured is null)
            {
                throw new InvalidOperationException("No backend address found");
            }
            return $"{unsecured}/{APIPrefix}/{apiSubPath}";
        }
        return $"{secured}/{APIPrefix}/{apiSubPath}";
    }

    public static string BlogAclUrl(this IBackendFixture fixture)
        => fixture.ApiUrl(BlogAclPath);

    public static string BlogSpotUrl(this IBackendFixture fixture)
        => fixture.ApiUrl(BlogSpotPath);

    public static string PublishUrl(this IBackendFixture fixture, long id)
        => fixture.ApiUrl(BlogSpotPath) +"/" + id + "/publish";

}
