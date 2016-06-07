using System.IO;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.StaticFiles;

namespace Yavsc
{

    public partial class Startup
    {
        public static string UserFilesDirName { get; private set; }
        public void ConfigureFileServerApp(IApplicationBuilder app,
                SiteSettings siteSettings, IHostingEnvironment env)
        {
            app.UseFileServer(new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(
                        env.WebRootPath,
                        // TODO: add a ressource serveur id here, 
                        // or remove the blog entry id usage, to use the userid instead
                        // and an user defined optional subdir.
                        siteSettings.UserFiles.DirName
                    )),
                RequestPath = new PathString("/" + siteSettings.UserFiles.DirName),
                EnableDirectoryBrowsing = false
            });
            app.UseStaticFiles();
        }
    }
}
