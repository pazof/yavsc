using Microsoft.EntityFrameworkCore;
using Xunit;
using Yavsc;
using Yavsc.Abstract.Workflow;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Haircut;
using Yavsc.Services;

namespace yavscTests
{
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

            var firstRegistrar = new Func<ApplicationDbContext, long, IDecidableQuery>((db, id) =>
                db.HairCutQueries.Include(q => q.Prestation).Include(q => q.Regularisation).Single(q => q.Id == id));

            const string testCode = "TestBrush";
            
            Assert.Throws<InvalidOperationException>(() =>
                WorkflowHelpers.RegisterBilling<HairCutQuery>(testCode, firstRegistrar));
        }
    }
}
