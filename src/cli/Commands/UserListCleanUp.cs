using cli.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using cli.Services;
using Yavsc.Server.Settings;
using Yavsc.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System;

namespace cli.Commands
{
    public class UserListCleanUp : ICommander
    {
        public CommandLineApplication Integrate(CommandLineApplication rootApp)
        {
            CommandOption sendHelpOption = null;
            CommandLineApplication userCleanupCommandApp
               = rootApp.Command("user-cleanup",
                   (target) =>
                   {
                       target.FullName = "Remove invalid users";
                       target.Description = "Remove who didn't confirmed their e-mail in 14 days";
                       sendHelpOption = target.HelpOption("-? | -h | --help");
                   }, false);

            userCleanupCommandApp.OnExecute(async () =>
            {
                bool showhelp = sendHelpOption.HasValue();

                if (!showhelp)
                {
                    var host = new WebHostBuilder();

                    var hostengnine = host.UseEnvironment(Program.HostingEnvironment.EnvironmentName)
                        .UseServer("cli")
                        .UseStartup<Startup>()
                        .Build();
                    var app = hostengnine.Start();

                    var mailer = app.Services.GetService<EMailer>();
                    var loggerFactory = app.Services.GetService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<Program>();
                    var userManager = app.Services.GetService<UserManager<ApplicationUser>>();
                    ApplicationDbContext dbContext = app.Services.GetService<ApplicationDbContext>();
                    Func<ApplicationUser, bool> criteria = UserPolicies.Criterias["user-to-remove"];

                    if (userManager==null)
                    {
                        logger.LogError("No user manager");
                        throw new Exception("No user manager");
                    }

                    logger.LogInformation("Starting emailling");
                    try {
                        mailer.SendEmailFromCriteria("user-to-remove");
                    }
                    catch (NoMaillingTemplateException ex)
                    {
                        logger.LogWarning(ex.Message);
                    }
                    ApplicationUser [] users = dbContext.ApplicationUser.Where(
                            u => criteria(u)).ToArray();
                    foreach (ApplicationUser user in users)
                    {
                        dbContext.DeviceDeclaration.RemoveRange(dbContext.DeviceDeclaration.Where(g => g.DeviceOwnerId == user.Id));
                        await userManager.DeleteAsync(user);
                    }
                    logger.LogInformation("Finished user cleanup");
                }
                else
                {
                    userCleanupCommandApp.ShowHelp();
                    return 1;
                }
                return 0;
            });
            return userCleanupCommandApp;
        }
    }
}
