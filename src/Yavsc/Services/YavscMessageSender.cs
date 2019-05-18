using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json;
using Yavsc.Interfaces.Workflow;
using Yavsc.Models;
using Yavsc.Models.Google.Messaging;
using Yavsc.Models.Haircut;
using Yavsc.Models.Messaging;

namespace Yavsc.Services
{
    public class YavscMessageSender : IYavscMessageSender
    {
        private ILogger _logger;
        IEmailSender _emailSender;
        SiteSettings siteSettings;
        private IHubContext hubContext;
        ApplicationDbContext _dbContext;

        public YavscMessageSender(
            ILoggerFactory loggerFactory, 
            IOptions<SiteSettings> sitesOptions, 
            IOptions<SmtpSettings> smtpOptions,
            IEmailSender emailSender,
            ApplicationDbContext dbContext
        )
        { 
            _logger = loggerFactory.CreateLogger<MailSender>();
            _emailSender = emailSender;
            siteSettings = sitesOptions?.Value;
            hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            _dbContext = dbContext;
        }
        public async Task <MessageWithPayloadResponse> NotifyEvent<Event>
         (IEnumerable<string> userIds, Event ev)
           where Event : IEvent
        {

            if (ev == null)
                throw new Exception("Spécifier un évènement");

            if (ev.Sender == null)
                throw new Exception("Spécifier un expéditeur");

            if (userIds == null )
                throw new Exception("Notify e No user id");

            MessageWithPayloadResponse response = new MessageWithPayloadResponse();

            var raa = userIds.ToArray();
            if (raa.Length<1)
                 throw new Exception("No dest id");
            
            try {
                _logger.LogInformation($"Sending to {string.Join(" ",raa)} using signalR : "+JsonConvert.SerializeObject(ev));
                List<MessageWithPayloadResponse.Result> results = new List<MessageWithPayloadResponse.Result>();
                foreach(var clientId in raa) {
                    MessageWithPayloadResponse.Result result = new MessageWithPayloadResponse.Result();
                    result.registration_id = clientId;

                    var hubClient = hubContext.Clients.User(clientId);
                    if (hubClient == null)
                    {
                        var user = _dbContext.Users.FirstOrDefault(u=> u.Id == clientId);
                        if (user==null)
                        {
                            response.failure++;
                            result.error = "no such user.";
                            continue;
                        }
                        if (!user.EmailConfirmed){
                            response.failure++;
                            result.error = "user has not confirmed his email address.";
                            continue;
                        }
                        var body = ev.CreateBody();

                        var mailSent = await _emailSender.SendEmailAsync(
                            user.UserName,
                            user.Email,
                             $"{ev.Sender} (un client) vous demande un rendez-vous",
                            $"{ev.CreateBody()}\r\n"
                        );

                        var sent = await _emailSender.SendEmailAsync(ev.Sender, user.Email,
                            ev.Topic, body
                        );
                        if (!sent.Sent) {
                            result.message_id = sent.MessageId;
                            result.error = sent.ErrorMessage;
                            response.failure++;
                        }
                        else 
                        {
                            result.message_id = mailSent.MessageId;
                            response.success++;
                        }
                    }
                    else {
                        // we assume that each hub connected client will handle this signal
                        result.message_id=hubClient.notify(ev);
                        response.success++;
                    }
                    results.Add(result);
                }
                response.results = results.ToArray();
                return response;
            }
            catch (Exception ex) {
              _logger.LogError("Quelque chose s'est mal passé à l'envoi: "+ex.Message);  
              throw;
            }
        }
        
        public async Task<MessageWithPayloadResponse> NotifyBookQueryAsync( IEnumerable<string> registrationIds, RdvQueryEvent ev)
        {
            return await NotifyEvent<RdvQueryEvent>(registrationIds, ev);
        }

        public async Task<MessageWithPayloadResponse> NotifyEstimateAsync(IEnumerable<string> registrationIds, EstimationEvent ev)
        {
            return await NotifyEvent<EstimationEvent>(registrationIds, ev);
        }

        public async Task<MessageWithPayloadResponse> NotifyHairCutQueryAsync(
         IEnumerable<string> registrationIds, HairCutQueryEvent ev)
        {
            return await NotifyEvent<HairCutQueryEvent>(registrationIds, ev);
        }

        public async Task<MessageWithPayloadResponse> NotifyAsync(IEnumerable<string> regids, IEvent yaev)
        {
            return await NotifyEvent<IEvent>(regids, yaev);
        }
        
        /* SMS with Twilio:
public Task SendSmsAsync(TwilioSettings twilioSettigns, string number, string message)
{
var Twilio = new TwilioRestClient(twilioSettigns.AccountSID, twilioSettigns.Token);
var result = Twilio.SendMessage( twilioSettigns.SMSAccountFrom, number, message);
// Status is one of Queued, Sending, Sent, Failed or null if the number is not valid
Trace.TraceInformation(result.Status);
// Twilio doesn't currently have an async API, so return success.

return Task.FromResult(result.Status != "Failed");

}  */
    }
}
