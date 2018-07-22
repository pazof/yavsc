using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.Logging;

namespace Yavsc
{
    using Microsoft.Data.Entity;
    using Models;
    using Yavsc.Abstract.Workflow;
    using Yavsc.Billing;
    using Yavsc.Models.Billing;
    using Yavsc.Models.Haircut;
    using Yavsc.Models.Workflow;
    using Yavsc.Services;

    public partial class Startup
    {
        /// <summary>
        /// Lists Available user profile classes,
        /// populated at startup, using reflexion.
        /// </summary>
        public static List<Type> ProfileTypes = new List<Type>();

        
        /// <summary>
        /// Lists available command forms.
        /// This is hard coded.
        /// </summary>
        public static readonly string[] Forms = new string[] { "Profiles", "HairCut" };

        private void ConfigureWorkflow(IApplicationBuilder app, SiteSettings settings, ILogger logger)
        {
          //  System.AppDomain.CurrentDomain.ResourceResolve += OnYavscResourceResolve;

            try {
                foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var c in a.GetTypes())
                {
                    if (c.IsClass && !c.IsAbstract &&
                        c.GetInterface("ISpecializationSettings") != null)
                    {
                        ProfileTypes.Add(c);
                    }
                }
            }

            }
            catch (Exception ex)
            {
                logger.LogError(ex.TargetSite.Name);
            }
            

            foreach (var propinfo in typeof(ApplicationDbContext).GetProperties())
            {
                foreach (var attr in propinfo.CustomAttributes)
                {
                    // something like a DbSet?
                    if (attr.AttributeType == typeof(Yavsc.Attributes.ActivitySettingsAttribute))
                    {
                        // TODO swith () case {}
                        if (typeof(IQueryable<ISpecializationSettings>).IsAssignableFrom(propinfo.PropertyType))
                        {// double-bingo 
                            logger.LogVerbose($"Pro: {propinfo.Name}");
                            BillingService.UserSettings.Add(propinfo);
                        }
                        else
                        // Design time error
                        {
                            var msg =
    $@"La propriété {propinfo.Name} du contexte de la
base de donnée porte l'attribut [ActivitySetting],
mais n'implemente pas l'interface IQueryable<ISpecializationSettings>
({propinfo.MemberType.GetType()})";
                            logger.LogCritical(msg);
                        }
                    }
                }
            }

            RegisterBilling<HairCutQuery>(BillingCodes.Brush, new Func<ApplicationDbContext,long,INominativeQuery>
            ( ( db, id) => 
            {
              var query = db.HairCutQueries.Include(q=>q.Prestation).Include(q=>q.Regularisation).Single(q=>q.Id == id)  ; 
              query.SelectedProfile = db.BrusherProfile.Single(b=>b.UserId == query.PerformerId);
              return query;
            })) ;

            RegisterBilling<HairMultiCutQuery>(BillingCodes.MBrush,new Func<ApplicationDbContext,long,INominativeQuery>
            ( (db, id) =>  db.HairMultiCutQueries.Include(q=>q.Regularisation).Single(q=>q.Id == id)));
            RegisterBilling<RdvQuery>(BillingCodes.Rdv, new Func<ApplicationDbContext,long,INominativeQuery>
            ( (db, id) =>  db.RdvQueries.Include(q=>q.Regularisation).Single(q=>q.Id == id)));
        }
        public static System.Reflection.Assembly OnYavscResourceResolve(object sender, ResolveEventArgs ev)
        {
            return AppDomain.CurrentDomain.GetAssemblies()[0];
        }

        public static void RegisterBilling<T>(string code, Func<ApplicationDbContext,long,INominativeQuery> getter) where T : IBillable
        {
            BillingService.Billing.Add(code,getter) ;
            BillingService.GlobalBillingMap.Add(typeof(T).Name,code);
        }
    }

}
