using System;
using System.Linq;
using System.IO;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.CSharp;
using Yavsc.Models;
using Yavsc.Models.Identity;
using System.Reflection;
using Yavsc.Templates;
using Yavsc.Abstract.Templates;
using Microsoft.AspNet.Identity.EntityFramework;

namespace cli.Services
{

    public class EMailer
    {
        RazorTemplateEngine razorEngine;
        IStringLocalizer<EMailer> stringLocalizer;
        ILogger logger;
        ApplicationDbContext dbContext;

        const string DefaultBaseClassName = "ATemplate";
        const string DefaultBaseClass = nameof(UserOrientedTemplate);
        const string DefaultNamespace = "CompiledRazorTemplates";

        RazorEngineHost host;
        public EMailer(ApplicationDbContext context, IStringLocalizer<EMailer> localizer, ILoggerFactory loggerFactory)
        {
            stringLocalizer = localizer;

            logger = loggerFactory.CreateLogger<EMailer>();

            var language = new CSharpRazorCodeLanguage();

            host = new RazorEngineHost(language)
            {
                DefaultBaseClass = DefaultBaseClass,
                DefaultClassName = DefaultBaseClassName,
                DefaultNamespace = DefaultNamespace
            };


            // Everyone needs the System namespace, right?
            host.NamespaceImports.Add("System");
            host.NamespaceImports.Add("Yavsc.Templates");
            host.NamespaceImports.Add("Yavsc.Models");
            host.NamespaceImports.Add("Yavsc.Models.Identity");
            host.NamespaceImports.Add("Microsoft.AspNet.Identity.EntityFramework");
            host.InstrumentedSourceFilePath = "bin/output/approot/src/";
            host.StaticHelpers = true;
            dbContext = context;

            this.razorEngine = new RazorTemplateEngine(host);
        }

        public string GenerateTemplateObject(string baseclassName = DefaultBaseClassName)
        {
            throw new NotImplementedException();
        }

        public void AllUserGen(long templateCode, string baseclassName = DefaultBaseClassName)
        {
            string className = "Generated" + baseclassName;

            string subtemp = stringLocalizer["MonthlySubjectTemplate"].Value;

            logger.LogInformation($"Generating {subtemp}[{className}]");

            var templateInfo = dbContext.MailingTemplate.FirstOrDefault(t => t.Id == templateCode);

            logger.LogInformation($"Using code: {templateCode} and subject: {subtemp} ");

            using (StringReader reader = new StringReader(templateInfo.Body))
            {

                // Generate code for the template
                var razorResult = razorEngine.GenerateCode(reader, className, DefaultNamespace, "fakeFileName.cs");

                logger.LogInformation("Razor exited " + (razorResult.Success ? "Ok" : "Ko") + ".");

                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(razorResult.GeneratedCode);

                string assemblyName = Path.GetRandomFileName();
                MetadataReference[] references = new MetadataReference[]
                {
                MetadataReference.CreateFromFile( typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile( typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile( typeof(IdentityUser).Assembly.Location),
                MetadataReference.CreateFromFile( typeof(ApplicationUser).Assembly.Location),
                MetadataReference.CreateFromFile( typeof(Template).Assembly.Location)
                };

                var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithModuleName("Yavsc.Absctract").WithModuleName("Yavsc")
                    .WithAllowUnsafe(true).WithOptimizationLevel(OptimizationLevel.Release)
                    .WithOutputKind(OutputKind.DynamicallyLinkedLibrary).WithPlatform(Platform.AnyCpu);

                CSharpCompilation compilation = CSharpCompilation.Create(
                    assemblyName,
                    syntaxTrees: new[] { syntaxTree },
                    references: references,
                    options: compilationOptions);


                foreach (var mref in references) logger.LogInformation($"ctor used ref to {mref.Display}[{mref.Properties.Kind}]");


                using (var ms = new MemoryStream())
                {
                    EmitResult result = compilation.Emit(ms);
                    foreach (Diagnostic diagnostic in result.Diagnostics.Where(diagnostic =>
                            diagnostic.Severity < DiagnosticSeverity.Error && !diagnostic.IsWarningAsError))
                    {
                        logger.LogWarning("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                        logger.LogWarning("{0}: {1}", diagnostic.Id, diagnostic.Location.GetLineSpan());
                    }
                    if (!result.Success)
                    {
                        logger.LogInformation(razorResult.GeneratedCode);
                        IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                            diagnostic.IsWarningAsError ||
                            diagnostic.Severity == DiagnosticSeverity.Error);
                        foreach (Diagnostic diagnostic in failures)
                        {
                            logger.LogCritical("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                            logger.LogCritical("{0}: {1}", diagnostic.Id, diagnostic.Location.GetLineSpan());
                        }
                    }
                    else
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        Assembly assembly = Assembly.Load(ms.ToArray());

                        Type type = assembly.GetType(DefaultNamespace + "." + className);
                        var generatedtemplate = (UserOrientedTemplate)Activator.CreateInstance(type);
                        foreach (var user in dbContext.ApplicationUser)
                        {
                            logger.LogInformation("Generation for " + user.UserName);
                            generatedtemplate.Init();
                            generatedtemplate.User = user;

                            logger.LogInformation(generatedtemplate.GeneratedText);
                        }

                        /* ... type.InvokeMember("Write",
                            BindingFlags.Default | BindingFlags.InvokeMethod,
                            null,
                            model,
                            new object[] { "Hello World" }); */


                    }
                }
            }

        }
    }
}
