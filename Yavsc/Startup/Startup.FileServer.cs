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

        public static FileServerOptions AvatarsOptions { get; set; }
        public void ConfigureFileServerApp(IApplicationBuilder app,
                SiteSettings siteSettings, IHostingEnvironment env)
        {
            var userFilesDirInfo = new DirectoryInfo( siteSettings.UserFiles.Blog );
            UserFilesDirName =  userFilesDirInfo.FullName;

            if (!userFilesDirInfo.Exists) userFilesDirInfo.Create();

            UserFilesOptions = new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(UserFilesDirName),
                RequestPath = new PathString(Constants.UserFilesPath),
                EnableDirectoryBrowsing = env.IsDevelopment()
            };
            var avatarsDirInfo = new DirectoryInfo(Startup.SiteSetup.UserFiles.Avatars);
            if (!avatarsDirInfo.Exists) avatarsDirInfo.Create();
            AvatarsDirName = avatarsDirInfo.FullName;

            AvatarsOptions = new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(AvatarsDirName),
                RequestPath = new PathString(Constants.AvatarsPath),
                EnableDirectoryBrowsing = env.IsDevelopment()
            };

            app.UseFileServer(UserFilesOptions);

            app.UseFileServer(AvatarsOptions);

            app.UseStaticFiles();
        }
    }
}
