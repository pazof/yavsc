using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Workflow;
using Yavsc.Models;

namespace Yavsc.Services
{
    public class BillingService : IBillingService
    {
        public ApplicationDbContext DbContext { get; private set; }
        public static Dictionary<string, Func<ApplicationDbContext, long, IDecidableQuery>> Billing =
        new Dictionary<string, Func<ApplicationDbContext, long, IDecidableQuery>>();
        public static List<PropertyInfo> UserSettings = new List<PropertyInfo>();

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

        public Task<IDecidableQuery> GetBillAsync(string billingCode, long queryId)
        {
            return Task.FromResult(GetBillable(DbContext, billingCode, queryId));
        }

        public static IDecidableQuery GetBillable(ApplicationDbContext context, string billingCode, long queryId) => Billing[billingCode](context, queryId);
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
