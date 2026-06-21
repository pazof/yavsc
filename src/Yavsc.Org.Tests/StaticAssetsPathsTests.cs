using System.IO;
using Xunit;
using Xunit.v3;

namespace Yavsc.Org.Tests;

/// <summary>
/// Diagnostic-only test that confirms the static-assets manifests
/// the WebServerFixture relies on are present in the test bin.
///
/// The MSBuild target <c>CopyYavscOrgStaticAssets</c> in
/// Yavsc.Org.Tests.csproj mirrors the Yavsc.Org static-assets
/// manifests (<c>Yavsc.Org.staticwebassets.endpoints.json</c> and
/// <c>Yavsc.Org.staticwebassets.runtime.json</c>) from
/// <c>../Yavsc.Org/bin/$(Configuration)/$(TargetFramework)/</c> into
/// the test output directory. The fixture then calls
/// <c>_app.MapStaticAssets(testRuntimeManifest)</c> with the
/// <c>runtime.json</c> path so the test host resolves the manifest
/// explicitly by file location, bypassing the default
/// {AssemblyName}.staticwebassets.* lookup (which would look for
/// <c>Yavsc.Org.Tests.staticwebassets.*</c>, files we don't produce).
///
/// This test is a guard for that build-time copy: if it ever fails
/// or is removed, the WebServerFixture will throw
/// "The static resources manifest file ... was not found" at boot.
/// Once the WebServerFixture is stable in CI, this test can be
/// deleted.
/// </summary>
public class StaticAssetsPathsTests
{
    private readonly ITestOutputHelper _output;

    public StaticAssetsPathsTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void YavscOrg_staticwebassets_manifests_are_mirrored_into_test_bin()
    {
        var testBin = AppContext.BaseDirectory;
        _output.WriteLine($"Test bin (AppContext.BaseDirectory) = {testBin}");

        var runtimeManifest = Path.Combine(testBin,
            "Yavsc.Org.staticwebassets.runtime.json");
        var endpointsManifest = Path.Combine(testBin,
            "Yavsc.Org.staticwebassets.endpoints.json");

        _output.WriteLine($"runtime manifest:   exists = {File.Exists(runtimeManifest)}, path = {runtimeManifest}");
        _output.WriteLine($"endpoints manifest: exists = {File.Exists(endpointsManifest)}, path = {endpointsManifest}");

        Assert.True(File.Exists(runtimeManifest),
            $"Expected {runtimeManifest} to be copied by CopyYavscOrgStaticAssets, but it is missing. " +
            "Check that the Yavsc.Org project builds before Yavsc.Org.Tests so the source manifest exists " +
            "and that the <Target> in Yavsc.Org.Tests.csproj is wired up correctly.");

        Assert.True(File.Exists(endpointsManifest),
            $"Expected {endpointsManifest} to be copied by CopyYavscOrgStaticAssets, but it is missing. " +
            "The runtime manifest alone is not enough — MapStaticAssets() may also read the endpoints " +
            "manifest depending on the SDK version.");
    }
}
