using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.StaticFiles;
using Microsoft.Extensions.Logging;
using Yavsc.Helpers;
using Yavsc.Services;
using Yavsc.ViewModels.Auth;

namespace Yavsc
{

    public static class YaFileSServerExtenstions 
    {
        public static IApplicationBuilder UseYaFileServer(this IApplicationBuilder builder, FileServerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (options.EnableDefaultFiles)
            {
                builder = builder.UseDefaultFiles(options.DefaultFilesOptions);
            }
            if (options.EnableDirectoryBrowsing)
            {
                builder = builder.UseDirectoryBrowser(options.DirectoryBrowserOptions);
            }
            return builder.UseYaSendFileFallback().UseStaticFiles(options.StaticFileOptions);
        }

        public static IApplicationBuilder UseYaSendFileFallback(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<YaSendFileMiddleware>(new object[0]);
        }
    }

    public partial class Startup
    {
        public static FileServerOptions UserFilesOptions { get; private set; }
        public static FileServerOptions GitOptions { get; private set; }

        public static FileServerOptions AvatarsOptions { get; set; }

        static IAuthorizationService AuthorizationService { get; set; }

        public void ConfigureFileServerApp(IApplicationBuilder app,
                SiteSettings siteSettings, IHostingEnvironment env,
                 IAuthorizationService authorizationService)
        {
            AuthorizationService = authorizationService;
            var userFilesDirInfo = new DirectoryInfo(siteSettings.Blog);
            AbstractFileSystemHelpers.UserFilesDirName = userFilesDirInfo.FullName;

            if (!userFilesDirInfo.Exists) userFilesDirInfo.Create();

            UserFilesOptions = new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(AbstractFileSystemHelpers.UserFilesDirName),
                RequestPath = PathString.FromUriComponent(Constants.UserFilesPath),
                EnableDirectoryBrowsing = env.IsDevelopment(),
            };
            UserFilesOptions.EnableDefaultFiles = true;
            UserFilesOptions.StaticFileOptions.ServeUnknownFileTypes = true;

            UserFilesOptions.StaticFileOptions.OnPrepareResponse = OnPrepareUserFileResponse;

            var avatarsDirInfo = new DirectoryInfo(Startup.SiteSetup.Avatars);
            if (!avatarsDirInfo.Exists) avatarsDirInfo.Create();
            AvatarsDirName = avatarsDirInfo.FullName;

            AvatarsOptions = new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(AvatarsDirName),
                RequestPath = PathString.FromUriComponent(Constants.AvatarsPath),
                EnableDirectoryBrowsing = env.IsDevelopment()
            };


            var gitdirinfo = new DirectoryInfo(Startup.SiteSetup.GitRepository);
            GitDirName = gitdirinfo.FullName;
            if (!gitdirinfo.Exists) gitdirinfo.Create();
            GitOptions = new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(GitDirName),
                RequestPath = PathString.FromUriComponent(Constants.GitPath),
                EnableDirectoryBrowsing = env.IsDevelopment(),
            };
            GitOptions.DefaultFilesOptions.DefaultFileNames.Add("index.md");
            GitOptions.StaticFileOptions.ServeUnknownFileTypes = true;
            _logger.LogInformation($"{GitDirName}");
            GitOptions.StaticFileOptions.OnPrepareResponse += OnPrepareGitRepoResponse;

            app.UseFileServer(UserFilesOptions);

            app.UseFileServer(AvatarsOptions);

            app.UseFileServer(GitOptions);
            app.UseStaticFiles();
        }
        

        private async void OnPrepareUserFileResponse(StaticFileResponseContext context)
        {
            var uname = context.Context.User?.GetUserName();
            var path = context.Context.Request.Path;
            var result = await AuthorizationService.AuthorizeAsync(context.Context.User,
                new ViewFileContext{ UserName = uname, File = context.File, Path = path }, new ViewRequirement());
            if (!result)
            {
                _logger.LogInformation("403");
                // TODO prettier
                context.Context.Response.StatusCode = 403;
                context.Context.Response.Redirect("/Home/Status/403", false);
            }
        }

        static void OnPrepareGitRepoResponse(StaticFileResponseContext context)
        {
            if (context.File.Name.EndsWith(".ansi.log"))
            {
                context.Context.Response.Redirect("/Git" + context.Context.Request.Path);
            }
        }
    }
}
