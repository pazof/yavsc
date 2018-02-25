using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Yavsc.Abstract.Workflow;
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

        public Task<INominativeQuery> GetBillAsync(string billingCode, long queryId)
        {
            return Task.FromResult(Startup.GetBillable(DbContext,billingCode,queryId));
        }

        public async Task<ISpecializationSettings> GetPerformerSettingsAsync(string activityCode, string userId)
        {
            return await (await GetPerformersSettingsAsync(activityCode)).SingleOrDefaultAsync(s=> s.UserId == userId);
        }

        public async Task<IQueryable<ISpecializationSettings>> GetPerformersSettingsAsync(string activityCode)
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
    }
}