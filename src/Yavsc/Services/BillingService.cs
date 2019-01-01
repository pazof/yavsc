using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Yavsc.Abstract.Workflow;
using Yavsc.Models;
using Microsoft.Data.Entity;

namespace Yavsc.Services
{
    public class BillingService : IBillingService
    {
        public ApplicationDbContext DbContext { get; private set; }
        private ILogger logger;
        public static Dictionary<string,Func<ApplicationDbContext,long,IDecidableQuery>> Billing =
        new Dictionary<string,Func<ApplicationDbContext,long,IDecidableQuery>> ();
        public static List<PropertyInfo> UserSettings = new List<PropertyInfo>();
        
        public static Dictionary<string,string> GlobalBillingMap =
          new Dictionary<string,string>();

        public Dictionary<string,string> BillingMap {
          get { return GlobalBillingMap; }
        }

        public BillingService(ILoggerFactory loggerFactory, ApplicationDbContext dbContext)
        {
            logger = loggerFactory.CreateLogger<BillingService>();
            DbContext = dbContext;
        }

        public Task<IDecidableQuery> GetBillAsync(string billingCode, long queryId)
        {
            return Task.FromResult(GetBillable(DbContext,billingCode,queryId));
        }

        public static IDecidableQuery GetBillable(ApplicationDbContext context, string billingCode, long queryId ) =>  Billing[billingCode](context, queryId);
        public async Task<ISpecializationSettings> GetPerformerSettingsAsync(string activityCode, string userId)
        {
            return await (await GetPerformersSettingsAsync(activityCode)).SingleOrDefaultAsync(s=> s.UserId == userId);
        }

        public async Task<IQueryable<ISpecializationSettings>> GetPerformersSettingsAsync(string activityCode)
        {
            var activity = await DbContext.Activities.SingleAsync(a=>a.Code == activityCode);
           
            if (activity.SettingsClassName==null) return null;
            var dbSetPropInfo = UserSettings.SingleOrDefault(s => s.PropertyType.GenericTypeArguments[0].FullName == activity.SettingsClassName);
  
            if (dbSetPropInfo == null) return null;

            return (IQueryable<ISpecializationSettings>) dbSetPropInfo.GetValue(DbContext);
        }
    }
}
