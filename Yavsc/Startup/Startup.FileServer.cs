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
        public static FileServerOptions UserFilesOptions { get; private set; }

        public void ConfigureFileServerApp(IApplicationBuilder app,
                SiteSettings siteSettings, IHostingEnvironment env)
        {
            var userFilesDirInfo = new DirectoryInfo( siteSettings.UserFiles.DirName );
            UserFilesDirName =  userFilesDirInfo.FullName;

            if (!userFilesDirInfo.Exists) userFilesDirInfo.Create();

            UserFilesOptions = new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(UserFilesDirName),
                RequestPath = new PathString("/" + siteSettings.UserFiles.DirName),
                EnableDirectoryBrowsing = env.IsDevelopment()
            };
            app.UseFileServer(UserFilesOptions);
            app.UseStaticFiles();
        }
    }
}
