using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.Logging;
using Yavsc.Models;
using YavscLib;

namespace Yavsc
{
    public partial class Startup
    {
        /// <summary>
        /// Lists Available user profile classes,
        /// populated at startup, using reflexion.
        /// </summary>
        public static List<Type> ProfileTypes = new List<Type>() ;
        public static List<PropertyInfo> UserSettings = new List<PropertyInfo> ();

        /// <summary>
        /// Lists available command forms. 
        /// This is hard coded.
        /// </summary>
        public static readonly string [] Forms = new string [] { "Profiles" , "HairCut" };

        private void ConfigureWorkflow(IApplicationBuilder app, SiteSettings settings, ILogger logger)
        {
            System.AppDomain.CurrentDomain.ResourceResolve += OnYavscResourceResolve;

            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var c in a.GetTypes()) {
                    if (c.IsClass && !c.IsAbstract &&
                        c.GetInterface("ISpecializationSettings")!=null) {
                        ProfileTypes.Add(c);
                    }
                }
            }
                foreach (var propinfo in typeof(ApplicationDbContext).GetProperties()) {
                    foreach (var attr in propinfo.CustomAttributes) {
                        if (attr.AttributeType.FullName == "Yavsc.Attributes.ActivitySettingAttribute") {
                            // bingo
                            if (typeof(IQueryable<ISpecializationSettings>).IsAssignableFrom(propinfo.PropertyType))
                            {
                                logger.LogInformation($"Paramêtres utilisateur déclaré: {propinfo.Name}");
                                UserSettings.Add(propinfo);
                                
                            } else 
                                // Design time error
                                {
                                    logger.LogCritical(
$@"la propriété {propinfo.Name} du contexte de la
base de donnée déclare être un refuge de paramêtre utilisateur
du workflow, mais l'implemente pas l'interface IQueryable<ISpecializationSettings>,
Elle est du type {propinfo.MemberType.GetType()}");
                                    throw new NotSupportedException("invalid ActivitySettingAttribute on property from dbcontext");
                                }
                        }
                    }
                }
        }
        public static System.Reflection.Assembly OnYavscResourceResolve (object sender,  ResolveEventArgs ev)
        {
            return AppDomain.CurrentDomain.GetAssemblies()[0];
        } 
    }
    
}
