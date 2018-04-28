using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;

using System.Globalization;
using System.Reflection;
// using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
// using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Localization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNet.Razor;
using Microsoft.Extensions.DependencyInjection.Abstractions;
using Microsoft.Extensions.PlatformAbstractions;
using cli.Services;
using Yavsc;
using Yavsc.Models;
using Yavsc.Server.Helpers;
using Yavsc.Services;
using Yavsc.Templates;

namespace cli
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var host = new WebHostBuilder();

            var hostengnine = host
            .UseEnvironment("Development")
            .UseServer("cli")
            .UseStartup<Startup>()
            .Build();

            var app = hostengnine.Start();
            var mailer = app.Services.GetService<EMailer>();
            mailer.AllUserGen(2, nameof(UserOrientedTemplate));
        }
    }
}
