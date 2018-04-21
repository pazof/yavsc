using System;
using Microsoft.AspNet.Razor;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Yavsc.Models;

namespace cli.Services
{
    public class EMailer
    {
        RazorTemplateEngine razorEngine;
        IStringLocalizer<EMailer> stringLocalizer;
        ILogger logger;
        ApplicationDbContext dbContext;


        public EMailer(ApplicationDbContext context, RazorTemplateEngine razorTemplateEngine, IStringLocalizer<EMailer> localizer, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<EMailer>();

            razorEngine = razorTemplateEngine;

            stringLocalizer = localizer;

            dbContext = context;
        }

        public string Gen(long templateCode)
        {
            string subtemp = stringLocalizer["MonthlySubjectTemplate"].Value;
            logger.LogInformation ( $"Using code: {templateCode} and subject: {subtemp} " );
            throw new NotImplementedException("razorEngine.GenerateCode");
        }
    }
}
