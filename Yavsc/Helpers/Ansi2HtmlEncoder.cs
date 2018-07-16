// // AnsiToHtmlEncoder.cs
// /*
// paul schneider <paul@pschneider.fr> 19/06/2018 15:58 20182018 6 19
// */

using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Yavsc.Helpers
{
    public static class AnsiToHtmlEncoder
    {
        const string DocStart = "<!doctype html>\n<html>\n<head>"+
        "<style>body {\ncolor: grey;\nbackground-color: black;\nfont-family:fixed;}\n</style>\n</head>"+
        "<body><pre><code>\n";
        const string DocEnd = "</code></pre></body></html>";

        public static Stream GetStream(StreamReader reader)
        {
            var procStart = new ProcessStartInfo("/usr/bin/nodejs", "node_modules/ansi-to-html/bin/ansi-to-html");
            procStart.UseShellExecute = false;
            procStart.RedirectStandardInput = true;
            procStart.RedirectStandardOutput = true;
          //  procStart.RedirectStandardError = true;
            var mem = new MemoryStream();
            var writer = new StreamWriter(mem);
            var proc = Process.Start(procStart);


            Task.Run(() => {
                while (reader.Peek()>-1)
                 {
                     proc.StandardInput.WriteLine(reader.ReadLine());
                 }
                 proc.StandardInput.Close();
            });
                
                writer.WriteLine(DocStart);
                while (proc.StandardOutput.Peek()>-1)
                 {
                     writer.WriteLine(proc.StandardOutput.ReadLine());
                 } 
                writer.WriteLine(DocEnd);
                writer.Flush();
                mem.Seek(0,SeekOrigin.Begin);
              
            return mem;
        }

        public static Stream GetStream(Stream inner)
        {
            var reader = new StreamReader(inner);
            return GetStream(reader);
        }
    }
}