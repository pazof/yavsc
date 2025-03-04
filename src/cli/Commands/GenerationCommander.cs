using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.OptionsModel;
using cli.Model;
using cli.Services;
using cli.Settings;

namespace cli.Commands
{
    public class GenerationCommander : ICommander
    {

        public GenerationCommander()
        {
        }

        public CommandLineApplication Integrate(CommandLineApplication rootApp)
        {
            CommandArgument nameSpaceArg=null;
            CommandArgument mdClass=null;
            CommandArgument ctrlName=null;
            CommandArgument rPath=null;
            CommandArgument dbCtx=null;

            var cmd = rootApp.Command("mvc", config => {
                config.FullName = "mvc controller";
                config.Description = "generates an mvc controller";
                nameSpaceArg = config.Argument("ns","default name space");
                mdClass = config.Argument("mc","Model class name");
                ctrlName = config.Argument("cn", "Controller name");
                rPath = config.Argument("rp", "Relative path");
                config.HelpOption("-? | -h | --help");
            });
            cmd.OnExecute(() => {
                var host = new WebHostBuilder();
                    var hostEngine = host.UseEnvironment("Development")
                        .UseServer("cli")
                        .UseStartup<Startup>()
                        .Build();
                    var app = hostEngine.Start();
                    var mailer = app.Services.GetService<MvcGenerator>();
                    var loggerFactory = app.Services.GetService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<GenerationCommander>();
                    var options = app.Services.GetService<IOptions<GenMvcSettings>>();

                    MvcGenerator generator = app.Services.GetService<MvcGenerator>();

                    var modelFullName = mdClass?.Value ?? options?.Value.ModelFullName;
                    var nameSpace = nameSpaceArg?.Value?? options?.Value.NameSpace;
                    var dbContext = dbCtx?.Value?? options?.Value.DbContextFullName;
                    var controllerName = ctrlName?.Value?? options?.Value.ControllerName;
                    var relativePath = rPath?.Value ?? options?.Value.RelativePath;

                    logger.LogInformation("Starting generation");
                    logger.LogInformation($"Using parameters : modelFullName:{modelFullName} nameSpace:{nameSpace} dbContext:{dbContext} controllerName:{controllerName} relativePath:{relativePath}");

                    generator.Generate(modelFullName,
                     dbContext, 
                     controllerName,
                     relativePath);
               
                    logger.LogInformation("Finished generation");

                return 0;
            });
            return cmd;
        }
    }
}
