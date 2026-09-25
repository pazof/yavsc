using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Workflow;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Haircut;
using Yavsc.Services;
using Xunit;

namespace Yavsc
{
    // BillingService and Config expose static mutable registries that
    // WorkflowHelpers.ConfigureBillingService() also mutates from the
    // WebServerFixture host build (HostingExtensions.cs). Without a
    // collection, xUnit runs this class in parallel with the
    // "Yavsc Server" collection, and the host build's ConfigureBilling
    // call can land between this test's two ConfigureBilling calls,
    // breaking the idempotency count assertion (a flaky failure that
    // surfaces whenever the fixture's timing shifts). Pinning the class
    // to the same collection serializes it with the host build.
    [Collection("Yavsc Server")]
    [Trait("regression", "II")]
    public class BillingServiceTests
    {
        [Fact]
        public void ConfigureBillingService_CanBeCalledTwiceWithoutThrowing()
        {
            // First initialization should populate the billing registry.
            WorkflowHelpers.ConfigureBillingService();

            int firstBillingCount = BillingService.Billing.Count;
            int firstSettingsCount = BillingService.UserSettings.Count;
            int firstProfileTypesCount = Config.ProfileTypes.Count;

            // Second call should be idempotent and not throw.
            WorkflowHelpers.ConfigureBillingService();

            Assert.Equal(firstBillingCount, BillingService.Billing.Count);
            Assert.Equal(firstSettingsCount, BillingService.UserSettings.Count);
            Assert.Equal(firstProfileTypesCount, Config.ProfileTypes.Count);
        }

        [Fact]
        public void RegisterBilling_DuplicateRegistrationThrowsInvalidOperationException()
        {
            WorkflowHelpers.ConfigureBillingService();

            var firstRegistrar = new Func<ApplicationDbContext, long, IQuery>((db, id) =>
                db.HairCutQueries.Include(q => q.Prestation).Include(q => q.Regularization).Single(q => q.Id == id));

            const string testCode = "Brush";

            Assert.Throws<InvalidOperationException>(() =>
                WorkflowHelpers.RegisterBilling<HairCutQuery>(testCode, firstRegistrar));
        }
    }
}
