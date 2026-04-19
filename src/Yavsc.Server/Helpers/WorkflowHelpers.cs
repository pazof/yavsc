
namespace Yavsc.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Yavsc.Abstract.Workflow;
    using Yavsc.Billing;
    using Yavsc.Models;
    using Yavsc.Models.Billing;
    using Yavsc.Models.Haircut;
    using Yavsc.Models.Workflow;
    using Yavsc.Services;
    using Yavsc.ViewModels.FrontOffice;

    public static class WorkflowHelpers
    {
        // Synchronization lock for billing service configuration
        private static readonly object _billingLock = new object();

        public static async Task<List<PerformerProfileViewModel>>
        ListPerformersAsync(this ApplicationDbContext context,
        IBillingService billing,
        string actCode)
        {

            var actors = context.Performers
           .Include(p => p.Activity)
           .Include(p => p.Performer)
           .Where(p => p.Active && p.Activity.Any(u => u.DoesCode == actCode)).OrderBy(x => x.Rate)
           .ToArray();

            List<PerformerProfileViewModel> result = new();
            foreach (var a in actors)
            {
                var settings = await billing.GetPerformersSettingsAsync(actCode, a.PerformerId);
                result.Add(new PerformerProfileViewModel(a, actCode, settings));
            }

            return result;
        }

        public static void RegisterBilling<T>(string code, Func<ApplicationDbContext, long,
        IDecidableQuery> getter) where T : IBillable
        {
            lock (_billingLock)
            {
                string typeName = typeof(T).Name;
                if (BillingService.GlobalBillingMap.ContainsKey(typeName))
                {
                    throw new InvalidOperationException($"Billing setup: type '{typeName}' already registered with different code");
                }
                if (BillingService.Billing.ContainsKey(code))
                {
                    throw new InvalidOperationException($"Billing setup: code '{code}' already registered with different type");
                }
                BillingService.Billing.Add(code, getter);
            }
        }

        public static void ConfigureBillingService()
        {
            lock (_billingLock)
            {
                BillingService.Billing.Clear();
                BillingService.GlobalBillingMap.Clear();
                BillingService.UserSettings.Clear();
                Config.ProfileTypes.Clear();

                foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var c in a.GetTypes())
                    {
                        if (c.IsClass && !c.IsAbstract &&
                            c.GetInterface("ISpecializationSettings") != null)
                        {
                            Config.ProfileTypes.Add(c);
                        }
                    }
                }

                foreach (var propertyInfo in typeof(ApplicationDbContext).GetProperties())
                {
                    foreach (var attr in propertyInfo.CustomAttributes)
                    {
                        // something like a DbSet?
                        if (typeof(Yavsc.Attributes.ActivitySettingsAttribute).IsAssignableFrom(attr.AttributeType))
                        {
                            BillingService.UserSettings.Add(propertyInfo);
                        }
                    }
                }

                RegisterBilling<HairCutQuery>(BillingCodes.Brush, new Func<ApplicationDbContext, long, IDecidableQuery>
                ((db, id) =>
                {
                    var query = db.HairCutQueries.Include(q => q.Prestation).Include(q => q.Regularisation).Single(q => q.Id == id);
                    query.SelectedProfile = db.BrusherProfile.Single(b => b.UserId == query.PerformerId);
                    return query;
                }));

                RegisterBilling<HairMultiCutQuery>(BillingCodes.MBrush, new Func<ApplicationDbContext, long, IDecidableQuery>
                ((db, id) => db.HairMultiCutQueries.Include(q => q.Regularisation).Single(q => q.Id == id)));

                RegisterBilling<RdvQuery>(BillingCodes.Rdv, new Func<ApplicationDbContext, long, IDecidableQuery>
                ((db, id) => db.RdvQueries.Include(q => q.Regularisation).Single(q => q.Id == id)));
            }
        }

    }
}
