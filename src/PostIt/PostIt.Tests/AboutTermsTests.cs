using PostIt.ViewModels;

namespace PostIt.Tests;

public sealed class AboutTermsTests
{
    [Fact]
    public void Terms_follow_the_configured_instance_and_changes_to_it()
    {
        var settings = new Settings();
        settings.Authentication.Authority = "https://first.example/";
        var model = new AboutPageViewModel(settings);
        Assert.Equal("https://first.example/Home/CGU", model.TermsUri.AbsoluteUri);
        Assert.Equal("https://first.example/ResourceSubscription", model.SubscriptionsUri.AbsoluteUri);
        settings.Authentication.Authority = "https://second.example";
        Assert.Equal("https://second.example/Home/CGU", model.TermsUri.AbsoluteUri);
        Assert.Equal("https://second.example/ResourceSubscription", model.SubscriptionsUri.AbsoluteUri);
    }
}
