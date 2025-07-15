
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Controller;

namespace cli.Services
{
    
    public class MvcGenerator  : CommandLineGenerator
    {
        readonly CommandLineGeneratorModel _model;
        readonly ILogger _logger;
        public MvcGenerator(IServiceProvider services, ILoggerFactory loggerFactory) : base(services)
        {
            _model = new CommandLineGeneratorModel();
            _logger = loggerFactory.CreateLogger<MvcGenerator>();
        }

        public async void Generate(
            string modelClass, 
            string dbContextFullName,
            string controllerName,
            string relativeFolderPath
            )
        {
            _model.ControllerName = controllerName;
            _model.ModelClass = modelClass;
            _model.DataContextClass = dbContextFullName;
            _model.RelativeFolderPath = relativeFolderPath;

            _logger.LogInformation($"Generation for {_model.ModelClass} @ {_model.RelativeFolderPath}");

            await GenerateCode(_model);
        }
    }
}
