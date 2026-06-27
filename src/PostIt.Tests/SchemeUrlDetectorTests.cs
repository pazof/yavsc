using PostIt.Services;
using Xunit;

namespace PostIt.Tests;

/// <summary>
/// Tests for the platform-independent scheme-URL detector. The
/// detector is the first guard against the OS launching a fresh
/// PostIt instance with the postit://callback URL — it must match
/// even when Avalonia has not booted, otherwise the 2nd instance
/// flashes its own MainWindow before shutting down.
/// </summary>
public class SchemeUrlDetectorTests
{
    [Fact]
    public void FindCallbackUrl_returns_null_when_no_args()
    {
        Assert.Null(SchemeUrlDetector.FindCallbackUrl(System.Array.Empty<string>()));
    }

    [Fact]
    public void FindCallbackUrl_returns_null_when_no_postit_arg_present()
    {
        var args = new[]
        {
            "/usr/bin/postit-desktop",
            "--some-flag",
            "value",
        };
        Assert.Null(SchemeUrlDetector.FindCallbackUrl(args));
    }

    [Fact]
    public void FindCallbackUrl_returns_url_when_postit_scheme_present()
    {
        var args = new[]
        {
            "/usr/bin/postit-desktop",
            "postit://callback?code=abc&state=xyz",
        };
        var hit = SchemeUrlDetector.FindCallbackUrl(args);
        Assert.Equal("postit://callback?code=abc&state=xyz", hit);
    }

    [Fact]
    public void FindCallbackUrl_is_case_insensitive_on_scheme()
    {
        var args = new[] { "POSTIT://callback?code=abc" };
        Assert.Equal("POSTIT://callback?code=abc", SchemeUrlDetector.FindCallbackUrl(args));
    }

    [Fact]
    public void FindCallbackUrl_ignores_args_that_mention_scheme_without_prefix()
    {
        // "postit-something://x" must NOT match — the prefix is the
        // scheme followed by "://", nothing else.
        var args = new[] { "postit-something://callback?code=abc" };
        Assert.Null(SchemeUrlDetector.FindCallbackUrl(args));
    }

    [Fact]
    public void FindCallbackUrl_returns_first_match_when_multiple_present()
    {
        // Defensive: an OS shouldn't hand us two URLs in argv, but if
        // it ever does we want a deterministic answer (first).
        var args = new[]
        {
            "postit://callback?code=first",
            "postit://callback?code=second",
        };
        Assert.Equal("postit://callback?code=first", SchemeUrlDetector.FindCallbackUrl(args));
    }

    [Fact]
    public void FindCallbackUrl_returns_null_for_null_args()
    {
        Assert.Null(SchemeUrlDetector.FindCallbackUrl(null!));
    }
}
