using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;

public class InteractiveConsoleMiddleWare 
{
        public InteractiveConsoleMiddleWare(RequestDelegate next)
{
    _next = next;
}

readonly RequestDelegate _next;

public async Task Invoke(HttpContext context, IHostingEnvironment hostingEnviroment)
{
    //do something
    System.Console.WriteLine("kjhnlkhkl");
    await _next.Invoke(context);
}

}