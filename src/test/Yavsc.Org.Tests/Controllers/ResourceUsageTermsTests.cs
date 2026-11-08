using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Yavsc.Settings;
using Xunit;

namespace Yavsc.Org.Tests.Controllers;

public sealed class ResourceUsageTermsTests
{
    [Fact]
    public async Task Privacy_describes_withdrawal_payment_exception_and_actual_recipients()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/Home/Privacy", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("un an", html);
        Assert.Contains("retrait effectif", html);
        Assert.Contains("comptables", html);
        Assert.Contains("PayPal", html);
        Assert.Contains("Anthropic", html);
        Assert.Contains("OAuth", html);
        Assert.Contains("PostIt", html);
        Assert.DoesNotContain("Aucune information", html);
        Assert.DoesNotContain("mise en œuvre immédiate", html);
    }

    [Fact]
    public async Task Public_terms_describe_the_configured_rule_and_are_linked_from_the_footer()
    {
        using var factory = new TestWebApplicationFactory();
        var options = factory.Services.GetRequiredService<IOptions<ResourceUsageOptions>>().Value;
        options.DefaultLimit = 12345m;
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/Home/CGU", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("12345", html);
        Assert.Contains("resource_quota_exceeded", html);
        Assert.Contains("I =", html);
        Assert.Contains("Home/CGU", html);
        Assert.Contains((Yavsc.Constants.DefaultFSQ * 5L).ToString(), html);
        Assert.Contains((Yavsc.Constants.DefaultFSQ * 2L).ToString(), html);
        Assert.Contains("quota disque", html);
        Assert.Contains("Découverte porte le quota disque", html);
        Assert.Contains("remboursent les points de stockage", html);
        Assert.Contains("suppression de fichiers reste autorisée", html);
        Assert.DoesNotContain("ne remboursent pas de points", html);
    }
}
