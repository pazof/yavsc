using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.Logging;

namespace Yavsc
{
    using Microsoft.Data.Entity;
    using Models;
    using Yavsc.Billing;
    public partial class Startup
    {
        /// <summary>
        /// Lists Available user profile classes,
        /// populated at startup, using reflexion.
        /// </summary>
        public static List<Type> ProfileTypes = new List<Type>();
        public static List<PropertyInfo> UserSettings = new List<PropertyInfo>();

        public static Dictionary<string,Func<ApplicationDbContext,long,IBillable>> Billing =
        new Dictionary<string,Func<ApplicationDbContext,long,IBillable>> ();

        /// <summary>
        /// Lists available command forms.
        /// This is hard coded.
        /// </summary>
        public static readonly string[] Forms = new string[] { "Profiles", "HairCut" };

        private void ConfigureWorkflow(IApplicationBuilder app, SiteSettings settings, ILogger logger)
        {
            System.AppDomain.CurrentDomain.ResourceResolve += OnYavscResourceResolve;

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
                            UserSettings.Add(propinfo);
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

            Billing.Add("Brush", new Func<ApplicationDbContext,long,IBillable>
            ( ( db, id) => 
            {
              var query = db.HairCutQueries.Include(q=>q.Prestation).Include(q=>q.Regularisation).Single(q=>q.Id == id)  ; 
              query.SelectedProfile = db.BrusherProfile.Single(b=>b.UserId == query.PerformerId);
              return query;
            })) ;

            Billing.Add("MBrush",new Func<ApplicationDbContext,long,IBillable>
            ( (db, id) =>  db.HairMultiCutQueries.Include(q=>q.Regularisation).Single(q=>q.Id == id)));
            Billing.Add("Rdv", new Func<ApplicationDbContext,long,IBillable>
            ( (db, id) =>  db.RdvQueries.Include(q=>q.Regularisation).Single(q=>q.Id == id)));
        }
        public static System.Reflection.Assembly OnYavscResourceResolve(object sender, ResolveEventArgs ev)
        {
            return AppDomain.CurrentDomain.GetAssemblies()[0];
        }
    }

}
