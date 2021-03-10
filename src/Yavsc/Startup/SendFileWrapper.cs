using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Features;
using Microsoft.Extensions.Logging;

namespace Yavsc
{

    class YaSendFileWrapper : IHttpSendFileFeature
    {
        private readonly Stream _output;

        private readonly ILogger _logger;

        internal YaSendFileWrapper(Stream output, ILogger logger)
        {
            _output = output;
            _logger = logger;
        }

        public async Task SendFileAsync(string fileName, long offset, long? length, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(string.Empty, fileName);
            }
            FileInfo fileInfo = new FileInfo(fileName);
            if (offset < 0 || offset > fileInfo.Length)
            {
                throw new ArgumentOutOfRangeException("offset", offset, string.Empty);
            }
            if (length.HasValue && (length.Value < 0 || length.Value > fileInfo.Length - offset))
            {
                throw new ArgumentOutOfRangeException("length", length, string.Empty);
            }
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, maxbufferlen, FileOptions.Asynchronous | FileOptions.SequentialScan);
            try
            {
                fileStream.Seek(offset, SeekOrigin.Begin);
                _logger.LogInformation(string.Format("Copying bytes range:{0},{1} filename:{2} ", offset, (!length.HasValue) ? null : (offset + length), fileName));
                // await CopyToAsync(fileStream, _output, length, cancel);
                await CopyToAsync(fileStream, _output);
            }
            finally
            {
                fileStream.Dispose();
            }
        }

        private const int maxbufferlen = 65536;

        private async Task CopyToAsync(FileStream fileStream, Stream output)
        {
            await Task.Run(() => fileStream.CopyTo(output, maxbufferlen));
        }
    }
}
