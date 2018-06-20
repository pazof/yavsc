using System.IO;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.StaticFiles;
using Microsoft.Extensions.Logging;
using Yavsc.Abstract.FileSystem;
using Yavsc.ViewModels.Auth;

namespace Yavsc
{
    public partial class Startup
    {
        public static FileServerOptions UserFilesOptions { get; private set; }
        public static FileServerOptions GitOptions { get; private set; }

        public static FileServerOptions AvatarsOptions { get; set; }
        public void ConfigureFileServerApp(IApplicationBuilder app,
                SiteSettings siteSettings, IHostingEnvironment env, IAuthorizationService authorizationService)
        {
            var userFilesDirInfo = new DirectoryInfo( siteSettings.Blog );
            AbstractFileSystemHelpers.UserFilesDirName =  userFilesDirInfo.FullName;

            if (!userFilesDirInfo.Exists) userFilesDirInfo.Create();

            UserFilesOptions = new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(AbstractFileSystemHelpers.UserFilesDirName),
                RequestPath = new PathString(Constants.UserFilesPath),
                EnableDirectoryBrowsing = env.IsDevelopment(),
            };
            UserFilesOptions.EnableDefaultFiles=true;
            UserFilesOptions.StaticFileOptions.ServeUnknownFileTypes=true;

            UserFilesOptions.StaticFileOptions.OnPrepareResponse += async context =>
             {
                 var uname = context.Context.User.GetUserName();
                 var path = context.Context.Request.Path;
                 var result = await authorizationService.AuthorizeAsync(context.Context.User, new ViewFileContext
                 { UserName = uname, File = context.File, Path = path } , new ViewRequirement());
             };
            var avatarsDirInfo = new DirectoryInfo(Startup.SiteSetup.Avatars);
            if (!avatarsDirInfo.Exists) avatarsDirInfo.Create();
            AvatarsDirName = avatarsDirInfo.FullName;

            AvatarsOptions = new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(AvatarsDirName),
                RequestPath = new PathString(Constants.AvatarsPath),
                EnableDirectoryBrowsing = env.IsDevelopment()
            };


            var gitdirinfo = new DirectoryInfo(Startup.SiteSetup.GitRepository);
            GitDirName = gitdirinfo.FullName;
            if (!gitdirinfo.Exists) gitdirinfo.Create();
            GitOptions = new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(GitDirName),
                RequestPath = new PathString(Constants.GitPath),
                EnableDirectoryBrowsing = env.IsDevelopment(),
            };
            GitOptions.DefaultFilesOptions.DefaultFileNames.Add("index.md");
            GitOptions.StaticFileOptions.ServeUnknownFileTypes = true;
            logger.LogInformation( $"{GitDirName}");
            GitOptions.StaticFileOptions.OnPrepareResponse+= OnPrepareGitRepoResponse;

            app.UseFileServer(UserFilesOptions);

            app.UseFileServer(AvatarsOptions);

            app.UseFileServer(GitOptions);
            app.UseStaticFiles();
        }

        static void OnPrepareGitRepoResponse(StaticFileResponseContext context)
        {

            if (context.File.Name.EndsWith(".ansi.log"))
            {
                context.Context.Response.Redirect("/Git"+context.Context.Request.Path);
            }
        }
    }
}
