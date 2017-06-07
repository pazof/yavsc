
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Yavsc.Billing;
using Yavsc.Models;

namespace Yavsc.Services
{
    public class BillingService : IBillingService
    {
        public ApplicationDbContext DbContext { get; private set; }
        private ILogger logger;

        public BillingService(ILoggerFactory loggerFactory, ApplicationDbContext dbContext)
        {
            logger = loggerFactory.CreateLogger<BillingService>();
            DbContext = dbContext;
        }
        public async Task<IQueryable<ISpecializationSettings>> GetPerformersSettings(string activityCode)
        {
            logger.LogDebug("searching for "+activityCode);
            var activity = await DbContext.Activities.SingleAsync(a=>a.Code == activityCode);
            logger.LogDebug(JsonConvert.SerializeObject(activity));
  

            if (activity.SettingsClassName==null) return null;
            var dbSetPropInfo = Startup.UserSettings.SingleOrDefault(s => s.PropertyType.GenericTypeArguments[0].FullName == activity.SettingsClassName);
  

            if (dbSetPropInfo == null) return null;
            // var settingType = dbSetPropInfo.PropertyType;
            // var dbSetType = typeof(DbSet<>).MakeGenericType(new Type[] { settingType } );
            // avec une info method Remove et Update, ça le ferait ...

            return (IQueryable<ISpecializationSettings>) dbSetPropInfo.GetValue(DbContext);
        }

        public async Task<IBillable> GetBillAsync(string billingCode, long queryId)
        {
            var dbFunc = Startup.Billing[billingCode];
            IBillable query = null;
            await Task.Run(()=> dbFunc(DbContext, queryId));
            return query;
        }

        public async Task<ISpecializationSettings> GetPerformerSettingsAsync(string activityCode, string userId)
        {
            return await (await GetPerformersSettings(activityCode)).SingleOrDefaultAsync(s=> s.UserId == userId);
        }

        public Task<IQueryable<ISpecializationSettings>> GetPerformersSettingsAsync(string activityCode)
        {
            throw new NotImplementedException();
        }

       
    }
}