using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Workflow;
using Yavsc.Helpers;
using Yavsc.Models;

namespace Yavsc.Services
{
    public class BillingService : IBillingService
    {
        public ApplicationDbContext DbContext { get; private set; }
        public static Dictionary<string, Func<ApplicationDbContext, long, IQuery>> Billing =
        new Dictionary<string, Func<ApplicationDbContext, long, IQuery>>();
        public static List<PropertyInfo> UserSettings = new List<PropertyInfo>();

        /// <summary>
        /// Mapping from activity codes to IUserSettings
        /// </summary>
        public static Dictionary<string, string> GlobalBillingMap =
          new Dictionary<string, string>();

        public Dictionary<string, string> BillingMap
        {
            get { return GlobalBillingMap; }
        }

        public BillingService(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public Task<IQuery> GetBillAsync(string billingCode, long queryId)
        {
            return Task.FromResult(GetBillable(DbContext, billingCode, queryId));
        }

        public static IQuery GetBillable(ApplicationDbContext context, string billingCode, long queryId)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrWhiteSpace(billingCode) || queryId <= 0)
            {
                return null;
            }

            if (Billing.Count == 0)
            {
                WorkflowHelpers.ConfigureBillingService();
            }

            var getter = Billing
                .FirstOrDefault(kvp => string.Equals(kvp.Key, billingCode.Trim(), StringComparison.OrdinalIgnoreCase))
                .Value;

            if (getter is null)
            {
                return null;
            }

            try
            {
                return getter(context, queryId);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }


        public async Task<IUserSettings> GetPerformersSettingsAsync(string activityCode, string userId)
        {
            var activity = await DbContext.Activities.SingleAsync(a => a.Code == activityCode);

            if (activity.SettingsClassName == null) return null;

            var dbSetGetter =
            UserSettings.SingleOrDefault(s => s.Name == activity.SettingsClassName);

            if (dbSetGetter==null) return null;
            var dbSet = dbSetGetter.GetValue(DbContext);

            if (dbSet == null) return null;

            if (dbSet is DbSet<IUserSettings> userSettings)
            {
                return userSettings.FirstOrDefault(s => s.UserId == userId);
            }
            return null;
        }

    }
}
