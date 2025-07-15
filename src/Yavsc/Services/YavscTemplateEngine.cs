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
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using RazorEngine.Compilation.ImpromptuInterface.Optimization;
using RazorEngine.Compilation.ImpromptuInterface;

namespace Yavsc.Lib
{
    public class YavscTemplateEngine
    {
        ISet<string> Namespaces = new System.Collections.Generic.HashSet<string> {
            "System",
            "Yavsc.Templates" ,
            "Yavsc.Models",
            "Yavsc.Models.Identity"};
        
        readonly IStringLocalizer<YavscTemplateEngine> stringLocalizer;
        readonly ApplicationDbContext dbContext;

        readonly ILogger logger;

        public YavscTemplateEngine(ApplicationDbContext dbContext,
                       IStringLocalizer<YavscTemplateEngine> localizer,
                       ILoggerFactory loggerFactory)
        {
            stringLocalizer = localizer;
            this.dbContext = dbContext;

            logger = loggerFactory.CreateLogger<YavscTemplateEngine>();
        }

        public string RunUserTemplate(string templateCode)
        {

            string subtemp = stringLocalizer["MonthlySubjectTemplate"].Value;

            logger.LogInformation($"Generating SendMonthlyEmail {templateCode}");


            var templateInfo = dbContext.MailingTemplate.FirstOrDefault(t => t.Id == templateCode);
            Debug.Assert (templateInfo != null);
            var templatekey = RazorEngine.Engine.Razor.GetKey(templateInfo.Id);

            logger.LogInformation($"Using code: {templateCode},  subject: {subtemp} ");
            logger.LogInformation("And body:\n" + templateInfo.Body);


            // Generate code for the template
            using (var inMemoryCsharpCode = new MemoryStream())
            {
                using (var writter = new StreamWriter(inMemoryCsharpCode))
                {
                    RazorEngine.Engine.Razor.Run(templatekey, writter);
                    inMemoryCsharpCode.Seek(0, SeekOrigin.Begin);

                    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(Encoding.Default.GetString(inMemoryCsharpCode.ToArray()));

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

                    using (var inMemoryAssembly = new MemoryStream())
                    {
                        logger.LogInformation("Emitting result ...");
                        EmitResult result = compilation.Emit(inMemoryAssembly);
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
                            return null;
                        }
                        else
                        {
                               foreach (var user in dbContext.ApplicationUser.Where(
                                u => u.AllowMonthlyEmail
                            ))
                            {

                                var template = result.CallActLike<UserOrientedTemplate>(user);
                                return template.GeneratedText;
                            }
                           
/* result.CallActLike<>

 inMemoryAssembly.Seek(0, SeekOrigin.Begin);
                            Assembly assembly = Assembly.Load(inMemoryAssembly.ToArray());
                   //          UserOrientedTemplate userOrientedTemplate = (UserOrientedTemplate)
                   // FIXME               Activator.CreateInstance(Type.GetType(templateInfo.TemplateType));
                            
                            foreach (var user in dbContext.ApplicationUser.Where(
                                u => u.AllowMonthlyEmail
                            ))
                            {
                                logger.LogInformation("Generation for " + user.UserName);
                                userOrientedTemplate.Init();
                                userOrientedTemplate.User = user; */
                             throw new NotImplementedException();
                         
                        }
                    }
                }
            }
            return null;
        }
    }
}
