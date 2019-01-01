using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json;
using Yavsc.Interfaces.Workflow;
using Yavsc.Models.Google.Messaging;
using Yavsc.Models.Haircut;
using Yavsc.Models.Messaging;
using Yavsc.Server.Helpers;

namespace Yavsc.Services
{
    public class GCMSender : IGoogleCloudMessageSender
    {
        private ILogger _logger;
        SiteSettings siteSettings;
        GoogleAuthSettings googleSettings;

        public GCMSender(

            ILoggerFactory loggerFactory, 
            IOptions<SiteSettings> sitesOptions, 
            IOptions<SmtpSettings> smtpOptions,
            IOptions<GoogleAuthSettings> googleOptions
        )
        { _logger = loggerFactory.CreateLogger<MailSender>();
            siteSettings = sitesOptions?.Value;
            googleSettings = googleOptions?.Value;

        }
        public  async Task <MessageWithPayloadResponse> NotifyEvent<Event>
         (  IEnumerable<string> regids, Event ev)
           where Event : IEvent
        {
            if (ev == null)
                throw new Exception("Spécifier un évènement");

            if (ev.Sender == null)
                throw new Exception("Spécifier un expéditeur");

            if (regids == null )
                throw new NotImplementedException("Notify & No GCM reg ids");
            var raa = regids.ToArray();
            if (raa.Length<1)
                 throw new NotImplementedException("No GCM reg ids");
            var msg = new MessageWithPayload<Event>()
            {
                data = ev,
                registration_ids = regids.ToArray()
            };
            _logger.LogInformation("Sendding to Google : "+JsonConvert.SerializeObject(msg));
            try {
                using (var m = new SimpleJsonPostMethod("https://gcm-http.googleapis.com/gcm/send",$"key={googleSettings.ApiKey}")) {
                    return await m.Invoke<MessageWithPayloadResponse>(msg);
                }
            }
            catch (Exception ex) {
                throw new Exception ("Quelque chose s'est mal passé à l'envoi",ex);
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