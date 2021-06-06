using System;
using Xunit;
using System.IO;
using System.Diagnostics;

namespace yavscTests
{

    /// <summary>
    /// Since node isn't any requirement by here,
    /// It may regress
    /// </summary>
    [Trait("regression", "allways")]
    public class NodeTests
    {
        void TestNodeJsForAnsitohtml ()
        {
            var procStart = new ProcessStartInfo("node", "node_modules/ansi-to-html/bin/ansi-to-html")
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var proc = Process.Start(procStart);
           proc.StandardInput.WriteLine("\x001b[30mblack\x1b[37mwhite");
           proc.StandardInput.Close();
           while (!proc.StandardOutput.EndOfStream)
            {
                Console.Write(proc.StandardOutput.ReadToEnd());
            }
        }
        
        void AnsiToHtml()
        {
            var procStart = new ProcessStartInfo("ls", "-l --color=always")
            {
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = true
            };
            var proc = Process.Start(procStart);
            var encoded = GetStream(proc.StandardOutput); 
            var reader = new StreamReader(encoded);
            var txt = reader.ReadToEnd();
            
            Console.WriteLine(txt);
        }

        public static Stream GetStream(StreamReader reader)
        {
            var procStart = new ProcessStartInfo("node", "node_modules/ansi-to-html/bin/ansi-to-html");
            procStart.UseShellExecute = false;
            procStart.RedirectStandardInput = true;
            procStart.RedirectStandardOutput = true;
            procStart.RedirectStandardError = true;
            var mem = new MemoryStream();
            StreamWriter writer = new StreamWriter(mem);
            var proc = Process.Start(procStart);
            var text = reader.ReadToEnd();
            proc.StandardInput.WriteLine(text);
            proc.StandardInput.Close();
            return proc.StandardOutput.BaseStream;
        }
    }
}
