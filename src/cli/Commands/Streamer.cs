using System.Net.WebSockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;

namespace cli {
  
    public class Streamer {
        private ClientWebSocket _client;
        private ILogger _logger;
        private ConnectionSettings _cxSettings;
        private UserConnectionSettings _userCxSettings;

        public Streamer(ILoggerFactory loggerFactory, 
            IOptions<ConnectionSettings> cxSettings,
            IOptions<UserConnectionSettings> userCxSettings
        )
        {
            _logger = loggerFactory.CreateLogger<Streamer>();
            _cxSettings = cxSettings.Value;
            _userCxSettings = userCxSettings.Value;
            _client = new ClientWebSocket();
            _client.Options.SetRequestHeader("Authorization", $"Bearer {_userCxSettings.AccessToken}");
        }
    }  
}