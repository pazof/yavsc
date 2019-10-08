using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using cli.Model;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Yavsc.Abstract;

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
            const int bufLen = Constants.WebSocketsMaxBufLen;
            byte [] buffer = new byte[bufLen+4*sizeof(int)];
            const int offset=0;
            int read = 0;
            /* 
            var reciving = Task.Run(async ()=> {
                 byte [] readbuffer = new byte[bufLen];
                 var rb = new ArraySegment<byte>(readbuffer, 0, bufLen);
                 bool continueReading = false;
                 do {
                    var result = await _client.ReceiveAsync(rb, _tokenSource.Token);
                    _logger.LogInformation($"received {result.Count} bytes");
                    continueReading = !result.CloseStatus.HasValue;
                 } while (continueReading);
            } ); */

            do {
                read = await stream.ReadAsync(buffer, offset + sizeof(int), bufLen);
                if (read>0) {
                    // assert sizeof(int)==4
                    buffer[3]= (byte) (read % 256);
                    var left = read / 256;
                    buffer[2]=  (byte) (left % 256);
                    left = left / 256;
                    buffer[1] = (byte) (left % 256);
                    left = left /256;
                    buffer[0]=(byte) (byte) (left % 256);
                    var segment = new ArraySegment<byte>(buffer, offset, read+4);
                

                    await _client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, _tokenSource.Token);
                     _logger.LogInformation($"sent {segment.Count} ");
                 }
                
            } while (read>0);
            // reciving.Wait();
             await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "EOF", _tokenSource.Token);
        }
    }  
}
