using System;
using RazorEngine.Templating;
using Yavsc.Models;
using Yavsc.Services;

namespace cli.Modules
{
    public class MonthlyEMailGenerator : IModule
    {
        IRazorEngineService engine;
        IEmailSender emailSender;

        ApplicationDbContext dbContext;
        public MonthlyEMailGenerator(ApplicationDbContext context, IRazorEngineService res, IEmailSender sender)
        {
            dbContext = context;
            engine = res;
            emailSender = sender;
          //  engine.AddTemplate(new Tem)
        }
        public void Run(string[] args)
        {
            Console.WriteLine($"Hello from second module using {engine}");
            string template = "Hello @Model.Name, welcome to RazorEngine!";
            var result = engine.RunCompile(template, "templateKey", null, new { Name = "World" });
            Console.WriteLine(result);
        }
    }
}