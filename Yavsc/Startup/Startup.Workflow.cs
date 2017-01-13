using System;
using System.Collections.Generic;
using Microsoft.AspNet.Builder;

namespace Yavsc
{
    public partial class Startup
    {
        /// <summary>
        /// Lists Available user profile classes.
        /// </summary>
        public static  Dictionary<string,Type> ProfileTypes = new Dictionary<string,Type>() ;
        private void ConfigureWorkflow(IApplicationBuilder app, SiteSettings settings)
        {
            System.AppDomain.CurrentDomain.ResourceResolve += OnYavscResourceResolve;

            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var c in a.GetTypes()) {
                    if (c.IsClass && !c.IsAbstract &&
                        c.GetInterface("ISpecializationSettings")!=null) {
                        ProfileTypes.Add(c.FullName,c);
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
