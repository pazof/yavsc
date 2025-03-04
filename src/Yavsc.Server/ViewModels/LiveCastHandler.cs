using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.FileSystem;
using Yavsc.Server.Helpers;

namespace Yavsc.ViewModels.Streaming
{
    public class LiveCastClient
    {
        public string UserName { get; set; }
        public WebSocket Socket { get; set; }
    }

    public class LiveEntryViewModel
    {
        public string UserName { get; set; }
        public string FlowId { get; set; }
    }

    public class LiveCastHandler : IDisposable
    {

        public WebSocket Socket { get; set; }
        public ConcurrentDictionary<string, WebSocket> Listeners { get; set; } = new ConcurrentDictionary<string, WebSocket>();

        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();

        public void Dispose()
        {
        }

        public async Task<FileReceivedInfo> ReceiveUserFile(ApplicationUser user, ILogger logger, string root, Queue<ArraySegment<byte>> queue, string destFileName, Func<bool> isEndOfInput)
        {
            // TODO lock user's disk usage for this scope,
            // this process is not safe at concurrent access.
            long usage = user.DiskUsage;

            var item = new FileReceivedInfo
            (root, AbstractFileSystemHelpers.FilterFileName(destFileName));

            var fi = new FileInfo(Path.Combine(root, item.FileName));
            if (fi.Exists)
            {
                item.Overridden = true;
                usage -= fi.Length;
            }
            
            logger.LogInformation("Opening the file");
            using (var dest = fi.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                logger.LogInformation("Appening to file");
                while (!isEndOfInput() || queue.Count > 0)
                {
                    if (queue.Count > 0)
                    {
                        var buffer = queue.Dequeue();

                        logger.LogInformation($"writing {buffer.Count} bytes...");

                        await dest.WriteAsync(buffer.Array, buffer.Offset, buffer.Count);
                        logger.LogInformation($"done.");
                        usage += buffer.Count;
                    }
                    if (usage >= user.DiskQuota) break;
                    if (queue.Count == 0 && !isEndOfInput())
                    {
                        await Task.Delay(100);
                    }
                }
                user.DiskUsage = usage;
                dest.Close();
            }
            if (usage >= user.DiskQuota)
            {
                item.QuotaOffense = true;
            }
            user.DiskUsage = usage;
            return item;
        }

    }

}
