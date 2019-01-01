

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace Yavsc {

  public class StreamingApiController { 

    ILogger _logger;

    public StreamingApiController (LoggerFactory loggerFactory)
    {
      _logger = loggerFactory.CreateLogger<StreamingApiController>();
      _logger.LogInformation
        ("created logger");
    }

    public async Task<IActionResult> GetStreamingToken()
    {
      _logger.LogInformation("Token asked");
      throw new NotImplementedException();
    }
    public async Task<IActionResult> GetLiveStreamingIndex()
    {
      _logger.LogInformation("GetLiveStreamingIndex");
      throw new NotImplementedException();
    }

  }

}
