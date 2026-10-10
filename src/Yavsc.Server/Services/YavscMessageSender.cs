using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Yavsc.Interface;
using Yavsc.Interfaces.Workflow;
using Yavsc.Models;
using Yavsc.Models.Google.Messaging;
using Yavsc.Models.Haircut;
using Yavsc.Models.Messaging;
using Yavsc.Server.Hubs;
using Yavsc.Abstract.Models.Messaging;

namespace Yavsc.Services
{
    public class YavscMessageSender : IYavscMessageSender
    {
        private readonly ILogger _logger;
        readonly ITrueEmailSender _emailSender;
        readonly SiteSettings siteSettings;
        readonly ApplicationDbContext _dbContext;
        readonly IConnexionManager _cxManager;
        private readonly IHubContext<ChatHub> hubContext;

        public YavscMessageSender(
            ILoggerFactory loggerFactory,
            IOptions<SiteSettings> sitesOptions,
            ITrueEmailSender emailSender,
            ApplicationDbContext dbContext,
            IConnexionManager cxManager,
            IHubContext<ChatHub> hubContext
        )
        {
            _logger = loggerFactory.CreateLogger<YavscMessageSender>();
            _emailSender = emailSender;
            siteSettings = sitesOptions?.Value;
            this.hubContext = hubContext;
            _dbContext = dbContext;
            _cxManager = cxManager;
        }

        public async Task<MessageWithPayloadResponse> NotifyEvent<Event>
         (IEnumerable<string> userIds, Event ev)
           where Event : IEvent
        {

            if (ev == null)
                throw new Exception("Spécifier un évènement");

            if (ev.Sender == null)
                throw new Exception("Spécifier un expéditeur");

            if (userIds == null)
                throw new Exception("Notify e No user id");

            MessageWithPayloadResponse response = new MessageWithPayloadResponse();

            var raa = userIds.ToArray();
            if (raa.Length < 1)
                throw new Exception("No dest id");

            try
            {
                List<MessageWithPayloadResponse.Result> results = new List<MessageWithPayloadResponse.Result>();
                foreach (var userId in raa)
                {
                    _logger.LogDebug($"For performer id : {userId}");
                    MessageWithPayloadResponse.Result result = new MessageWithPayloadResponse.Result
                    {
                        registration_id = userId
                    };

                    var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
                    results.Add(result);
                    if (user == null)
                    {
                        response.failure++;
                        result.error = "no such user.";
                        continue;
                    }
                    var body = ev.CreateBody();
                    var subject = ev is ClientEstimateSignedEvent signed
                        ? signed.Title
                        : $"{ev.Sender} (un client) vous demande un rendez-vous";

                    if (ev is RdvQueryEvent || ev is ClientEstimateSignedEvent)
                    {
                        _dbContext.Notification.Add(new Notification
                        {
                            title = subject.Length > 1024 ? subject[..1024] : subject,
                            body = body.Length > 512 ? body[..512] : body,
                            Target = $"user/{userId}",
                            tag = ev switch
                            {
                                ClientEstimateSignedEvent estimateEvent => $"estimate/{estimateEvent.EstimateId}/client-signed",
                                RdvQueryEvent rdvEvent => $"rdv/{rdvEvent.Id}",
                                _ => throw new InvalidOperationException("Unsupported persistent notification event.")
                            },
                            click_action = "Fermer"
                        });
                        await _dbContext.SaveChangesAsync(ev.Sender);
                    }

                    if (!user.EmailConfirmed || string.IsNullOrWhiteSpace(user.Email))
                    {
                        response.failure++;
                        result.error = !user.EmailConfirmed
                            ? "user has not confirmed his email address."
                            : "user has no legacy email address.";
                        _logger.LogWarning("Email notification not sent to {UserId}: {Reason}", userId, result.error);
                    }
                    else
                    {
                        result.message_id = await _emailSender.SendEmailAsync(user.UserName, user.Email,
                        subject,
                    body + Environment.NewLine);
                        response.success++;
                    }

                    var cxIds = _cxManager.GetConnexionIds(user.UserName);
                    if (cxIds == null)
                    {
                        _logger.LogDebug($"no cx to {user.UserName} <{user.Email}> ");
                    }
                    else
                    {
                        _logger.LogDebug($"Sending signal to {string.Join(" ", cxIds)}  : " + JsonConvert.SerializeObject(ev));

                        foreach (var cxId in cxIds)
                        {
                            // from usr asp.net Id : var hubClient = hubContext.Clients.User(userId);
                            var hubClient = hubContext.Clients.Client(cxId);
                            var data = new Dictionary<string, object>
                            {
                                ["event"] = JsonConvert.SerializeObject(ev)
                            };
                            await hubClient.SendAsync("push", ev.Topic, JsonConvert.SerializeObject(data));
                        }

                        result.message_id = MimeKit.Utils.MimeUtils.GenerateMessageId(
                            siteSettings.Authority
                        );

                        response.success++;
                    }
                }
                response.results = results.ToArray();
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Quelque chose s'est mal passé à l'envoi: " + ex.Message);
                throw;
            }
        }

        public async Task<MessageWithPayloadResponse> NotifyBookQueryAsync(IEnumerable<string> userIds, RdvQueryEvent ev)
        {
            return await NotifyEvent<RdvQueryEvent>(userIds, ev);
        }

        public async Task<MessageWithPayloadResponse> NotifyEstimateAsync(IEnumerable<string> userIds, EstimationEvent ev)
        {
            return await NotifyEvent<EstimationEvent>(userIds, ev);
        }

        public async Task<MessageWithPayloadResponse> NotifyHairCutQueryAsync(
         IEnumerable<string> userIds, HairCutQueryEvent ev)
        {
            return await NotifyEvent<HairCutQueryEvent>(userIds, ev);
        }

        public async Task<MessageWithPayloadResponse> NotifyAsync(IEnumerable<string> userIds, IEvent yavscEvent)
        {
            return await NotifyEvent<IEvent>(userIds, yavscEvent);
        }
    }
}
