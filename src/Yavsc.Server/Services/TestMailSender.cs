using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Yavsc.Interface;

namespace Yavsc.Services
{
    public class TestMailSender : ITrueEmailSender, IEmailSender
    {
        private readonly ILogger<TestMailSender> logger;

        public TestMailSender(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<TestMailSender>();
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            logger.LogInformation("[TestMailSender] SendEmailAsync to {Email} subject={Subject}", email, subject);
            return Task.CompletedTask;
        }

        public Task<string> SendEmailAsync(string name, string email, string subject, string htmlMessage)
        {
            logger.LogInformation("[TestMailSender] SendEmailAsync to {Email} subject={Subject} name={Name}", email, subject, name);
            return Task.FromResult($"test-message-{Guid.NewGuid()}");
        }
    }
}
