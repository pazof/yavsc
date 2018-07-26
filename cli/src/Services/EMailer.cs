using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.AspNet.Razor;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.Identity.EntityFramework;

using Yavsc.Models;
using Yavsc.Templates;
using Yavsc.Abstract.Templates;
using Yavsc.Services;
using Yavsc.Abstract.Manage;

namespace cli.Services
{

    public class EMailer
    {
        const string DefaultBaseClassName = "ATemplate";
        const string DefaultBaseClass = nameof(UserOrientedTemplate);
        const string DefaultNamespace = "CompiledRazorTemplates";

        RazorTemplateEngine razorEngine;
        IStringLocalizer<EMailer> stringLocalizer;
        ILogger logger;
        ApplicationDbContext dbContext;
        IEmailSender mailSender;
        RazorEngineHost host;

        public EMailer(ApplicationDbContext context, IEmailSender sender, IStringLocalizer<EMailer> localizer, ILoggerFactory loggerFactory)
        {
            stringLocalizer = localizer;
            mailSender = sender;

            logger = loggerFactory.CreateLogger<EMailer>();

            var language = new CSharpRazorCodeLanguage();

            host = new RazorEngineHost(language)
            {
                DefaultBaseClass = DefaultBaseClass,
                DefaultClassName = DefaultBaseClassName,
                DefaultNamespace = DefaultNamespace
            };

            host.NamespaceImports.Add("System");
            host.NamespaceImports.Add("Yavsc.Templates");
            host.NamespaceImports.Add("Yavsc.Models");
            host.NamespaceImports.Add("Yavsc.Models.Identity");
            host.NamespaceImports.Add("Microsoft.AspNet.Identity.EntityFramework");
            host.InstrumentedSourceFilePath = ".";
            host.StaticHelpers = true;
            dbContext = context;
            razorEngine = new RazorTemplateEngine(host);
        }


        public void SendMonthlyEmail(long templateCode, string baseclassName = DefaultBaseClassName)
        {
            string className = "Generated" + baseclassName;

            string subtemp = stringLocalizer["MonthlySubjectTemplate"].Value;

            logger.LogInformation($"Generating {subtemp}[{className}]");

            var templateInfo = dbContext.MailingTemplate.FirstOrDefault(t => t.Id == templateCode);

            if (templateInfo==null) throw new Exception($"No template found under id {templateCode}.");  
            logger.LogInformation($"Using code: {templateCode},  subject: {subtemp} ");
            logger.LogInformation("And body:\n"+templateInfo.Body);
            using (StringReader reader = new StringReader(templateInfo.Body))
            {
                
                // Generate code for the template
                var razorResult = razorEngine.GenerateCode(reader, className, DefaultNamespace, DefaultNamespace+".cs");

                logger.LogInformation("Razor exited " + (razorResult.Success ? "Ok" : "Ko") + ".");

                logger.LogInformation(razorResult.GeneratedCode);
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(razorResult.GeneratedCode);
                logger.LogInformation("CSharp parsed");
                List<MetadataReference> references = new List<MetadataReference>();

                foreach (var type in new Type[] { 
                    typeof(object),
                    typeof(Enumerable),
                    typeof(IdentityUser), 
                    typeof(ApplicationUser), 
                    typeof(Template), 
                    typeof(UserOrientedTemplate),
                    typeof(System.Threading.Tasks.TaskExtensions)
                 } )
                {
                    var location = type.Assembly.Location;
                    if (!string.IsNullOrWhiteSpace(location)) {
                        references.Add(
                            MetadataReference.CreateFromFile(location)
                        );
                        logger.LogInformation($"Assembly for {type.Name} found at {location}");
                    } else logger.LogWarning($"Assembly Not found for {type.Name}");
                }

                logger.LogInformation("Compilation creation ...");

                var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithAllowUnsafe(true).WithOptimizationLevel(OptimizationLevel.Debug)
                    .WithOutputKind(OutputKind.DynamicallyLinkedLibrary).WithPlatform(Platform.AnyCpu)
                    .WithUsings("Yavsc.Templates")
                    ;
                string assemblyName = DefaultNamespace;
                CSharpCompilation compilation = CSharpCompilation.Create(
                    assemblyName,
                    syntaxTrees: new[] { syntaxTree },
                    references: references,
                    options: compilationOptions
                    );

                using (var ms = new MemoryStream())
                {
                    logger.LogInformation("Emitting result ...");
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
                        logger.LogInformation(razorResult.GeneratedCode);
                        ms.Seek(0, SeekOrigin.Begin);
                        Assembly assembly = Assembly.Load(ms.ToArray());

                        Type type = assembly.GetType(DefaultNamespace + "." + className);
                        var generatedtemplate = (UserOrientedTemplate) Activator.CreateInstance(type);
                        if (generatedtemplate==null) {
                            logger.LogError("No generated template ... exiting.");
                            throw new InvalidOperationException("No generated template");
                        }
                        foreach (var user in dbContext.ApplicationUser.Where(
                            u => u.AllowMonthlyEmail
                        ))
                        {
                            logger.LogInformation("Generation for " + user.UserName);
                            generatedtemplate.Init();
                            generatedtemplate.User = user;
                            generatedtemplate.ExecuteAsync(); 
                            logger.LogInformation(generatedtemplate.GeneratedText);
                            EmailSentViewModel mailSentInfo = this.mailSender.SendEmailAsync
                                (user.UserName, user.Email, $"monthly email", generatedtemplate.GeneratedText).Result;
                            if (mailSentInfo==null) 
                                logger.LogError("No info on sending");
                            else if (!mailSentInfo.Sent)
                                logger.LogError($"{mailSentInfo.ErrorMessage}");
                            else 
                                logger.LogInformation($"mailId:{mailSentInfo.MessageId} \nto:{user.UserName}");


                            
                        }

                    }
                }
            }

        }
    }
}
