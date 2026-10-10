using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Api.Test.Fixtures;
using Yavsc.ApiControllers;
using Yavsc.Models;
using Yavsc.Models.Billing;

namespace Yavsc.Api.Test;

[Collection("Yavsc Api")]
public sealed class BillingSignatureNotificationTests(ApiWebServerFixture fixture) : IClassFixture<ApiWebServerFixture>
{
    [Theory]
    [InlineData("bob", "bob", true)]
    [InlineData("alice", "alice", false)]
    [InlineData("alice", "bob", false)]
    public async Task Json_signature_notifies_only_when_the_authenticated_client_signs(
        string caller, string signer, bool notifies)
    {
        fixture.ResetAndSeedActivityGraph();
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var estimate = new Estimate
        {
            ClientId = "bob", OwnerId = "alice", CommandType = BillingCodes.Rdv,
            Title = "Devis notification", Description = "Devis de test",
            AttachedFiles = new List<string>(), AttachedGraphics = new List<string>()
        };
        db.Estimates.Add(estimate);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var controller = ActivatorUtilities.CreateInstance<BillingController>(scope.ServiceProvider);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", caller) }, "test"))
            }
        };
        controller.Url = new SignatureUrlHelper();
        var result = await controller.Sign(estimate.Id, new SignatureSubmission
        {
            SignerUserId = signer, CoordinateMax = 10000, Strokes = new[] { 1, 5000, 5000 }
        }, TestContext.Current.CancellationToken);
        if (caller != signer)
        {
            Assert.IsType<ForbidResult>(result);
            Assert.Empty(db.Signatures);
        }
        else
            Assert.IsType<CreatedResult>(result);

        var delivery = fixture.Services.GetRequiredService<RecordingMessageDelivery>();
        if (notifies)
        {
            Assert.Equal("user/alice", Assert.Single(db.Notification).Target);
            Assert.Equal("alice@example.test", Assert.Single(delivery.Emails).Recipient);
            Assert.NotEqual(default, estimate.ClientValidationDate);
        }
        else
        {
            Assert.Empty(db.Notification);
            Assert.Empty(delivery.Emails);
        }
    }

    private sealed class SignatureUrlHelper : IUrlHelper
    {
        public ActionContext ActionContext => throw new NotSupportedException();
        public string? Action(UrlActionContext actionContext) => "/api/v1/bill/estimate/sign";
        public string? Content(string? contentPath) => throw new NotSupportedException();
        public bool IsLocalUrl(string? url) => throw new NotSupportedException();
        public string? Link(string? routeName, object? values) => throw new NotSupportedException();
        public string? RouteUrl(UrlRouteContext routeContext) => throw new NotSupportedException();
    }
}
