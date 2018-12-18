

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
    }

    public async Task<IActionResult> GetStreamingToken()
    {
      throw new NotImplementedException();
    }

  }

}
