using System.Collections.Generic;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Yavsc.ViewModels;
using Yavsc.Models;
using System.Linq;

namespace Yavsc.Api.Helpers
{
    public static class RequestHelpers
    {
        // Check for some apache proxy header, if any
        public static string ForHost(this HttpRequest request) {
            string host = request.Headers["X-Forwarded-For"];
            if (string.IsNullOrEmpty(host)) {
                host = request.Host.Value;
            } else { // Using X-Forwarded-For last address
                host = host.Split(',')
                    .Last()
                    .Trim();
            }
            return host;
        }
    }
}
