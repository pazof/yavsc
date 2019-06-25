using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using cli.Model;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Yavsc;

namespace cli {
  
    public class Streamer: ICommander {
        private ClientWebSocket _client;
        private ILogger _logger;
        private ConnectionSettings _cxSettings;
        private UserConnectionSettings _userCxSettings;
        private CommandOption _fileOption;
        private CommandArgument _flowIdArg;
        private CancellationTokenSource _tokenSource;

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

        public CommandLineApplication Integrate(CommandLineApplication rootApp)
        {
            CommandLineApplication streamCmd = rootApp.Command("stream",
                (target) =>
                {
                    target.FullName = "Stream to server";
                    target.Description = "Stream arbitrary binary data to your server channel";
                    _fileOption = target.Option("-f | --file", "use given file as input stream", CommandOptionType.SingleValue);
                    _flowIdArg = target.Argument("flowId", "Remote Id for this channel", false);
                    target.HelpOption("-? | -h | --help");
                });
            streamCmd.OnExecute(async() => await DoExecute());
            return streamCmd;
        }

        private async Task <int> DoExecute()
        {
            if (_fileOption.HasValue()){
                var fi = new FileInfo(_fileOption.Value());
                if (!fi.Exists) {
                    _logger.LogError("Input file doesn´t exist.");
                    return -2;
                }
                using (var stream = fi.OpenRead())
                {
                    await DoStream(stream);
                }
                return 0;
            }
            else 
            {
                using(var stream = Console.OpenStandardInput())
                {
                    await DoStream(stream);
                }
                return 0;
            }
        }
        async Task DoStream (Stream stream)
        {
            _tokenSource = new CancellationTokenSource();
            await _client.ConnectAsync(
                new Uri(_cxSettings.StreamingUrl+"/"+_flowIdArg.Value),
                _tokenSource.Token);
            const int bufLen = Constants.WebSocketsMaxBufLen;
            byte [] buffer = new byte[bufLen];
            const int offset=0;
            int read = 0;
            do {
                read = await stream.ReadAsync(buffer, offset, bufLen);
                var segment = new ArraySegment<byte>(buffer, offset, read);
                await _client.SendAsync(new ArraySegment<byte>(buffer),  
                WebSocketMessageType.Binary, false, _tokenSource.Token);
            } while (read>0);
        }
    }  
}