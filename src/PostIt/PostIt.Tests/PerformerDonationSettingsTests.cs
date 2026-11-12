using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
public sealed class PerformerDonationSettingsTests
{
    [Theory]
    [InlineData("performer@payments.example", "performer@payments.example")]
    [InlineData(" performer@payments.example ", "performer@payments.example")]
    [InlineData("", "")]
    public void Performer_editor_includes_the_paypal_recipient_in_the_save_payload(string value, string expected)
    {
        var vm = new PerformerConfigurationPageViewModel { DonationPayPalEmail = value };
        Assert.Equal(expected, vm.BuildPerformerProfileSettings().DonationPayPalEmail);
    }

    [AvaloniaFact]
    public async Task Performer_editor_loads_displays_edits_and_saves_the_private_recipient()
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), "postit-donation-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            await using var api = new PerformerApi(tokenPath);
            var vm = new PerformerConfigurationPageViewModel(api, new ActivityApiClient(api, api.Settings.ApiUrl));
            await vm.RefreshAsync();
            Assert.Equal("initial@payments.example", vm.DonationPayPalEmail);
            var page = new PerformerConfigurationPage { DataContext = vm };
            var input = page.FindControl<TextBox>("DonationPayPalEmailInput")!;
            Assert.Equal("initial@payments.example", input.Text);
            input.Text = "updated@payments.example";
            Assert.Equal("updated@payments.example", vm.DonationPayPalEmail);
            await vm.SaveAsync();
            Assert.Equal("updated@payments.example", api.Saved?.DonationPayPalEmail);
            input.Text = "";
            await vm.SaveAsync();
            Assert.Equal("", api.Saved?.DonationPayPalEmail);
        }
        finally
        {
            if (File.Exists(tokenPath)) File.Delete(tokenPath);
        }
    }

    private sealed class PerformerApi(string tokenPath) : YavscApiClient(new Settings
    {
        Authentication = new AuthenticationSettings
        {
            Authority = "https://org.example.test", ClientId = "postit", Scopes = ["openid", "api"]
        }
    }, new TokenStore(tokenPath))
    {
        public PerformerProfileSettings? Saved { get; private set; }

        public override Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            if (typeof(T) == typeof(List<ActivityInfo>))
                return Task.FromResult((T)(object)new List<ActivityInfo>());
            Assert.EndsWith("/account/performer-profile", path);
            if (method == HttpMethod.Put) Saved = Assert.IsType<PerformerProfileSettings>(body);
            return Task.FromResult((T)(object)new PerformerProfileSettings
            {
                PerformerId = "author", DonationPayPalEmail = "initial@payments.example"
            });
        }
    }
}
