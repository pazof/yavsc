

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
        public static async Task<List<PerformerProfileViewModel>> 
        ListPerformersAsync(this ApplicationDbContext context, 
        IBillingService billing,
        string actCode)
        {
           
            var actors = context.Performers
           .Include(p=>p.Activity)
           .Include(p=>p.Performer)
           .Where(p => p.Active && p.Activity.Any(u=>u.DoesCode==actCode)).OrderBy( x => x.Rate )
           .ToArray();

            List<PerformerProfileViewModel> result = new ();
            foreach (var a in actors)
            {
                var settings = await billing.GetPerformersSettingsAsync(actCode, a.PerformerId);
                result.Add(new PerformerProfileViewModel(a, actCode,settings));
            }

            return result;
        }

    public static void RegisterBilling<T>(string code, Func<ApplicationDbContext, long, 
    IDecidableQuery> getter) where T : IBillable
    {
        BillingService.Billing.Add(code, getter);
        BillingService.GlobalBillingMap.Add(typeof(T).Name, code);
    }

    public static void ConfigureBillingService()
    {
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
