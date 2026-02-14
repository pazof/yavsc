
using cli.Model;
using cli.Services;
using cli.Settings;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cli.Commands
{
    public class GenerationCommander : ICommander
    {
    private MvcGenerator mvcGenerator;
    private ILoggerFactory loggerFactory;
    private IOptions<GenMvcSettings> options;

    public GenerationCommander(
            MvcGenerator mvcGenerator,
            ILoggerFactory loggerFactory,
            IOptions<GenMvcSettings> options
            )
        {
            this.mvcGenerator = mvcGenerator;
            this.loggerFactory = loggerFactory;
            this.options = options;
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
             
                    var logger = loggerFactory.CreateLogger<GenerationCommander>();
                    var modelFullName = mdClass?.Value ?? options?.Value.ModelFullName;
                    var nameSpace = nameSpaceArg?.Value?? options?.Value.NameSpace;
                    var dbContext = dbCtx?.Value?? options?.Value.DbContextFullName;
                    var controllerName = ctrlName?.Value?? options?.Value.ControllerName;
                    var relativePath = rPath?.Value ?? options?.Value.RelativePath;

                    logger.LogInformation("Starting generation");
                    logger.LogInformation($"Using parameters : modelFullName:{modelFullName} nameSpace:{nameSpace} dbContext:{dbContext} controllerName:{controllerName} relativePath:{relativePath}");

                    mvcGenerator.Generate(modelFullName,
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
