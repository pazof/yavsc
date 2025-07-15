using System.Text;
using cli.Model;
using Microsoft.Extensions.CommandLineUtils;

namespace cli.Helpers
{
    public static class ConsoleHelpers
    {
        public static CommandLineApplication Integrate(this CommandLineApplication rootApp, ICommander commander)
        {
            return commander.Integrate(rootApp);
        }

        public static string GetPassword()
        {
            var pwd = new StringBuilder();
            while (true)
            {
                var len = pwd.ToString().Length;
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.Remove(len - 1, 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    pwd.Append(i.KeyChar);
                    Console.Write("*");
                }
            }
            return pwd.ToString();
        }

    }

}
