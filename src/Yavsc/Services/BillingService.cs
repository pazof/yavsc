using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Workflow;
using Yavsc.Models;

namespace Yavsc.Services
{
    public class BillingService : IBillingService
    {
        public ApplicationDbContext DbContext { get; private set; }
        public static Dictionary<string,Func<ApplicationDbContext,long,IDecidableQuery>> Billing =
        new Dictionary<string,Func<ApplicationDbContext,long,IDecidableQuery>> ();
        public static List<PropertyInfo> UserSettings = new List<PropertyInfo>();
        
        public static Dictionary<string,string> GlobalBillingMap =
          new Dictionary<string,string>();

        public Dictionary<string,string> BillingMap {
          get { return GlobalBillingMap; }
        }

        public BillingService(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public Task<IDecidableQuery> GetBillAsync(string billingCode, long queryId)
        {
            return Task.FromResult(GetBillable(DbContext,billingCode,queryId));
        }

        public static IDecidableQuery GetBillable(ApplicationDbContext context, string billingCode, long queryId ) =>  Billing[billingCode](context, queryId);
        public async Task<ISpecializationSettings> GetPerformerSettingsAsync(string activityCode, string userId)
        {
            return (await  GetPerformersSettingsAsync(activityCode)).SingleOrDefault(s=> s.UserId == userId);
        }

        public async Task<IEnumerable<ISpecializationSettings>> GetPerformersSettingsAsync(string activityCode)
        {
            var activity = await DbContext.Activities.SingleAsync(a=>a.Code == activityCode);
           
            if (activity.SettingsClassName==null) return null;

            return UserSettings.Where(s => s.PropertyType.GenericTypeArguments[0].FullName == activity.SettingsClassName)
            .Cast<ISpecializationSettings>();
        }
    }
}
