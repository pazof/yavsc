using System;
using System.IO;
using System.Net.WebSockets;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using cli.Model;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Yavsc.Abstract;

namespace cli {
  
 public class Streamer: ICommander {
        private readonly ClientWebSocket _client;
        private readonly ILogger _logger;
        private readonly ConnectionSettings _cxSettings;
        private readonly UserConnectionSettings _userCxSettings;
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
                    _logger.LogInformation("DoExecute from given file");
                    await DoStream(stream);
                }
                return 0;
            }
            else 
            {
                using(var stream = Console.OpenStandardInput())
                {
                    _logger.LogInformation("DoExecute from standard input");
                    await DoStream(stream);
                }
                return 0;
            }
        }
        async Task DoStream (Stream stream)
        {

            _tokenSource = new CancellationTokenSource();
            var url = _cxSettings.StreamingUrl+"/"+_flowIdArg.Value;

            _logger.LogInformation("Connecting to "+url);
            await _client.ConnectAsync(new Uri(url), _tokenSource.Token);
            _logger.LogInformation("Connected");
            const int bufLen = Yavsc.Constants.WebSocketsMaxBufLen;
            byte [] buffer = new byte[bufLen];
            const int offset=0;
            int read;
            bool lastFrame;

            WebSocketMessageType pckType = WebSocketMessageType.Binary;
            do
            {
                read = await stream.ReadAsync(buffer, offset, bufLen);
                lastFrame = read < Yavsc.Constants.WebSocketsMaxBufLen;
                ArraySegment<byte> segment = new ArraySegment<byte>(buffer, offset, read);
                await _client.SendAsync(segment, pckType, lastFrame, _tokenSource.Token);
                _logger.LogInformation($"sent {segment.Count} ");
            } while (!lastFrame);
            _logger.LogInformation($"Closing socket");
            await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "EOF", _tokenSource.Token);
        }
    }

    public class NStreamer: ICommander {
        private readonly ClientWebSocket _client;
        private readonly ILogger _logger;
        private readonly ConnectionSettings _cxSettings;
        private readonly UserConnectionSettings _userCxSettings;
        private CommandArgument _sourceArg;
        private CommandArgument _destArg;
        private CancellationTokenSource _tokenSource;

        public NStreamer(ILoggerFactory loggerFactory, 
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
            CommandLineApplication streamCmd = rootApp.Command("nstream",
                (target) =>
                {
                    target.FullName = "Stream to server";
                    target.Description = "Stream arbitrary binary data to your server channel";

                    _sourceArg = target.Argument("source", "Source file to send, use '-' for standard input", false);
                    _destArg = target.Argument("destination", "destination file name", false);
                   
                    target.HelpOption("-? | -h | --help");
                });
            streamCmd.OnExecute(async() => await DoExecute());
            return streamCmd;
        }

        private async Task <int> DoExecute()
        {
            
            if (_sourceArg.Value != "-")
            {
                var fi = new FileInfo(_sourceArg.Value);
                if (!fi.Exists) {
                    _logger.LogError("Input file doesn´t exist.");
                    return -2;
                }
                using (var stream = fi.OpenRead())
                {
                    _logger.LogInformation("DoExecute from given file");
                    await DoStream(stream);
                }
                return 0;
            }
            else
            {
                using(var stream = Console.OpenStandardInput())
                {
                    _logger.LogInformation("DoExecute from standard input");
                    await DoStream(stream);
                }
                return 0;
            }
        }
        async Task DoStream (Stream stream)
        {
            _tokenSource = new CancellationTokenSource();
            var url = _cxSettings.NStreamingUrl + "/" + HttpUtility.UrlEncode(_destArg.Value);

            _logger.LogInformation("Connecting to " + url);
            await _client.ConnectAsync(new Uri(url), _tokenSource.Token);
            _logger.LogInformation("Connected");
            const int bufLen = Yavsc.Constants.WebSocketsMaxBufLen;
            byte [] buffer = new byte[bufLen];
            const int offset=0;
            int read;
            bool lastFrame;

            WebSocketMessageType pckType = WebSocketMessageType.Binary;
            do
            {
                read = await stream.ReadAsync(buffer, offset, bufLen);
                lastFrame = read < Yavsc.Constants.WebSocketsMaxBufLen;
                ArraySegment<byte> segment = new ArraySegment<byte>(buffer, offset, read);
                await _client.SendAsync(segment, pckType, lastFrame, _tokenSource.Token);
                _logger.LogInformation($"sent {segment.Count} ");
            } while (!lastFrame);
            _logger.LogInformation($"Closing socket");
            await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "EOF", _tokenSource.Token);
        }
    }
}
