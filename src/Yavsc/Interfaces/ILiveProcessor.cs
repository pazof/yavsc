using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Yavsc.ViewModels.Streaming;

namespace Yavsc.Services
{
    public interface ILiveProcessor {
        /// <summary>
        /// instance keeping reference on
        /// all collections of casting and listenning websockets
        /// </summary>
        /// <value></value>
        ConcurrentDictionary<string, LiveCastHandler> Casters { get; }
        /// <summary>
        /// Try and accept websocket from aspnet http context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<bool> AcceptStream (HttpContext context);

        /// <summary>
        /// live cast entry point
        /// </summary>
        /// <value></value>
        PathString LiveCastingPath {get; set;}
    }
}