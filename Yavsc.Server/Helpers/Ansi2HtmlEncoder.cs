// // AnsiToHtmlEncoder.cs
// /*
// paul schneider <paul@pschneider.fr> 19/06/2018 15:58 20182018 6 19
// */

using System.IO;
using System.Diagnostics;
namespace Yavsc.Server.Helpers
{
    public static class AnsiToHtmlEncoder
    {
        public static Stream GetStream(StreamReader reader)
        {
            var procStart = new ProcessStartInfo("sh", "ansi2html.sh --bg=dark --palette=linux");
            procStart.UseShellExecute = false;
            procStart.RedirectStandardInput = true;
            procStart.RedirectStandardOutput = true;
            var mem = new MemoryStream();
            StreamWriter writer = new StreamWriter(mem);

            var proc = Process.Start(procStart);
            while (!reader.EndOfStream && !proc.StandardOutput.EndOfStream)
            {
                if (!reader.EndOfStream)
                    proc.StandardInput.WriteLine(reader.ReadLine());
                if (!proc.StandardOutput.EndOfStream)
                    writer.WriteLine(proc.StandardOutput.ReadLine());
            }

            mem.Seek(0, SeekOrigin.Begin);
            return mem;
        }

        public static Stream GetStream(Stream inner)
        {
            using (var reader = new StreamReader(inner))
            {
                return GetStream(reader);
            }
        }
    }
}