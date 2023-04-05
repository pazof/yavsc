using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Yavsc.Helpers;
using Yavsc.ViewModels.Auth;

namespace Yavsc
{


    public partial class Startup
    {
        public static FileServerOptions UserFilesOptions { get; private set; }
        public static FileServerOptions GitOptions { get; private set; }

        public static FileServerOptions AvatarsOptions { get; set; }

        static IAuthorizationService AuthorizationService { get; set; }

        public void ConfigureFileServerApp(IApplicationBuilder app,
                SiteSettings siteSettings, IWebHostEnvironment env,
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
            if (!result.Succeeded)
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
