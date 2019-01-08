using System;
using Microsoft.Extensions.CodeGenerators.Mvc.Controller;

namespace cli.Services
{
    
    public class MvcGenerator  : CommandLineGenerator
    {
        CommandLineGeneratorModel _model;
        public MvcGenerator (IServiceProvider services): base (services)
        {
            _model = new CommandLineGeneratorModel();
        }

        public async void Generate(
            string modelClass, 
            string ns, 
            string dbContextFullName,
            string controllerName,
            string relativeFolderPath
            )
        {
            _model.ControllerName = controllerName;
            _model.ModelClass = modelClass;
            _model.DataContextClass = dbContextFullName;
            _model.RelativeFolderPath = relativeFolderPath;
            await GenerateCode(_model);
        }
    }
}