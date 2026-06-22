/*
 Copyright (c) 2024 HigginsSoft, Alexander Higgins - https://github.com/alexhiggins732/

 Copyright (c) 2018, Brock Allen & Dominick Baier. All rights reserved.

 Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
 Source code and license this software can be found

 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.
*/

using Anthropic.SDK;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yavsc;
using Yavsc.Abstract.Interfaces;
using Yavsc.Helpers;
using Yavsc.Interface;
using Yavsc.Interfaces;
using Yavsc.Models;
using Yavsc.Server.Helpers;
using Yavsc.Services;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.Title = "API";

        var builder = WebApplication.CreateBuilder(args);

        builder.AddConfiguration("api");

        var services = builder.Services;

        // Anthropic client
        builder.Services.AddSingleton<AnthropicClient>(_ =>
            new AnthropicClient(
                new APIAuthentication(
                    builder.Configuration["ANTHROPIC_API_KEY"]
                    ?? throw new InvalidOperationException("ANTHROPIC_API_KEY manquante")
                )
            )
        );

        // Service de modération
        if (builder.Environment.IsDevelopment())
            builder.Services.AddScoped<IModerationService, MockModerationService>();
        else
            builder.Services.AddScoped<IModerationService, ClaudeModerationService>();


        // accepts any access token issued by identity server
        // adds an authorization policy for scope 'scope1'

        services
            .AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy
                        .RequireAuthenticatedUser()
                        .RequireClaim(JwtClaimTypes.Scope, new string[] { "api" });
                });
            })
            .AddYavscCors(builder.Configuration)
            .AddControllers();

        // accepts any access token issued by identity server
        services.AddAuthentication("Bearer")
                 .AddYavscJwtBearer(builder.Configuration);

        services.AddDbContext<ApplicationDbContext>(options =>

           options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        services.AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        });
        //
        services.AddTransient<Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser>, MailSender>();
        services.AddTransient<ITrueEmailSender, MailSender>()
        .AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender,
         MailSender>()
        .TryAddSingleton<ISmtpClientFactory, SmtpClientFactory>();
        services
           .AddTransient<IBillingService, BillingService>()
           .AddTransient<ICalendarManager, CalendarManager>();
        services.AddTransient<IFileSystemAuthManager, FileSystemAuthManager>();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = false;
        });

        builder.Services.AddDistributedMemoryCache();

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[] { "fr", "en", "pt" };
            options.SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
        });
        WorkflowHelpers.ConfigureBillingService();
        using (var app = builder.Build())
        {
            if (app.Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseCors("default")
                ;
            app.MapIdentityApi<ApplicationUser>().RequireAuthorization("ApiScope");
            app.MapDefaultControllerRoute();
            app.MapGet("/identity", (HttpContext context) =>
                new JsonResult(context?.User?.Claims.Select(c => new { c.Type, c.Value }))
            );

            app.UseSession();
            await app.RunAsync();
        }
    }
}
