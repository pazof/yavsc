using cli.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yavsc.Server.Settings;
using Yavsc.Models;
using Microsoft.AspNetCore.Identity;
using Yavsc.Services;

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
                   

                    var mailer = Program.AppHost.Services.GetService<MailSender>();
                    var loggerFactory = Program.AppHost.Services.GetService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<Program>();
                    var userManager = Program.AppHost.Services.GetService<UserManager<ApplicationUser>>();
                    ApplicationDbContext dbContext = Program.AppHost.Services.GetService<ApplicationDbContext>();
                    Func<ApplicationUser, bool> criteria = UserPolicies.Criterias["user-to-remove"];

                    if (userManager==null)
                    {
                        logger.LogError("No user manager");
                        throw new Exception("No user manager");
                    }

                    logger.LogInformation("Starting emailling");
                    try
                    {
                        mailer.SendEmailFromCriteria("user-to-remove");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex.Message);
                        throw;
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
