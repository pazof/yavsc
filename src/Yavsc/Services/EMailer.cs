using System.Text;
// // EMailer.cs
// /*
// paul  26/06/2018 12:18 20182018 6 26
// */
using Yavsc.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Localization;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

using Yavsc.Models;
using Yavsc.Services;
using System.Reflection;
using Yavsc.Abstract.Templates;
using Microsoft.AspNetCore.Identity;
using RazorEngine.Configuration;
using Yavsc.Interface;

namespace Yavsc.Lib
{
    public class EMailer
    {
        const string DefaultBaseClassName = "ATemplate";
        const string DefaultBaseClass = nameof(UserOrientedTemplate);
        ISet<string> Namespaces = new System.Collections.Generic.HashSet<string> {
            "System",
            "Yavsc.Templates" ,
            "Yavsc.Models",
            "Yavsc.Models.Identity"};

        readonly IStringLocalizer<EMailer> stringLocalizer;
        readonly ApplicationDbContext dbContext;

        readonly ILogger logger;

        public EMailer(ApplicationDbContext context, 
                       IStringLocalizer<EMailer> localizer,
                       ILoggerFactory loggerFactory)
        {
            stringLocalizer = localizer;

            logger = loggerFactory.CreateLogger<EMailer>();


            var templateServiceConfig = new TemplateServiceConfiguration()
            {
                BaseTemplateType = typeof(UserOrientedTemplate),
                Language = RazorEngine.Language.CSharp,
                Namespaces = Namespaces

            };

        }

        public void SendMonthlyEmail(string templateCode, string baseclassName = DefaultBaseClassName)
        {
            string className = "Generated" + baseclassName;

            string subtemp = stringLocalizer["MonthlySubjectTemplate"].Value;

            logger.LogInformation($"Generating {subtemp}[{className}]");


            var templateInfo = dbContext.MailingTemplate.FirstOrDefault(t => t.Id == templateCode);
            var templatekey = RazorEngine.Engine.Razor.GetKey(templateInfo.Id);


            logger.LogInformation($"Using code: {templateCode},  subject: {subtemp} ");
            logger.LogInformation("And body:\n" + templateInfo.Body);
            using (StringReader reader = new StringReader(templateInfo.Body))
            {

                // Generate code for the template
                using (var rzcode = new MemoryStream())
                {
                    using (var writter = new StreamWriter(rzcode))
                    {
                        RazorEngine.Engine.Razor.Run(templatekey, writter);
                        rzcode.Seek(0, SeekOrigin.Begin);

                        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(Encoding.Default.GetString(rzcode.ToArray()));


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
                 })
                        {
                            var location = type.Assembly.Location;
                            if (!string.IsNullOrWhiteSpace(location))
                            {
                                references.Add(
                                    MetadataReference.CreateFromFile(location)
                                );
                                logger.LogInformation($"Assembly for {type.Name} found at {location}");
                            }
                            else logger.LogWarning($"Assembly Not found for {type.Name}");
                        }

                        logger.LogInformation("Compilation creation ...");

                        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                            .WithAllowUnsafe(true).WithOptimizationLevel(OptimizationLevel.Debug)
                            .WithOutputKind(OutputKind.DynamicallyLinkedLibrary).WithPlatform(Platform.AnyCpu)
                            .WithUsings("Yavsc.Templates")
                            ;
                        string assemblyName = "EMailSenderTemplate";
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

                                Type type = assembly.GetType(Namespaces + "." + className);
                                var generatedtemplate = (UserOrientedTemplate)Activator.CreateInstance(type);
                                foreach (var user in dbContext.ApplicationUser.Where(
                                    u => u.AllowMonthlyEmail
                                ))
                                {
                                    logger.LogInformation("Generation for " + user.UserName);
                                    generatedtemplate.Init();
                                    generatedtemplate.User = user;
                                    generatedtemplate.ExecuteAsync();
                                    logger.LogInformation(generatedtemplate.GeneratedText);
                                }

                            }
                        }
                    }

                }
            }
        }
    }
}
