/*
 Copyright (c) 2024 HigginsSoft, Alexander Higgins - https://github.com/alexhiggins732/ 

 Copyright (c) 2018, Brock Allen & Dominick Baier. All rights reserved.

 Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information. 
 Source code and license this software can be found 

 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.
*/

using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Interface;
using Yavsc.Models;
using Yavsc.Services;
using Yavsc.Server.Helpers;
using Microsoft.AspNetCore.Identity;
namespace Yavsc.Blogs;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.Title = "Yavsc.Blogs";

        var builder = WebApplication.CreateBuilder(args);

        builder.AddConfiguration("blogs");

        var services = builder.Services;

        // MvcBuilder
        builder.Services
            .AddAuthorization(options =>
            {
                options.AddPolicy("BlogScope", policy =>
            {
                policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(JwtClaimTypes.Scope, new string[] { "blog" });
            });
            })
            .AddYavscCors(builder.Configuration)
            .AddControllers();

        // AuthenticationBuilder
        services.AddAuthentication("Bearer")
                 .AddYavscJwtBearer(builder.Configuration);

        // DbContextBuilder
        services.AddDbContext<ApplicationDbContext>(options =>
           options.UseNpgsql(builder.Configuration.GetConnectionString(YavscConstants.YavscConnectionStringName)));

        // other services
        services
        .AddTransient<ITrueEmailSender, MailSender>()
        .AddTransient<IEmailSender<ApplicationUser>, MailSender>()
        .AddTransient<IBillingService, BillingService>()
        .AddTransient<ICalendarManager, CalendarManager>()
        .AddTransient<IFileSystemAuthManager, FileSystemAuthManager>()
        .AddTransient<BlogSpotService>()
        .AddScoped<IAuthorizationHandler, PermissionHandler>()
        .AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        })
        .AddDistributedMemoryCache()
        .AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = false;
        }).Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[] { "fr", "en", "pt" };
            options.SetDefaultCulture(supportedCultures[0])
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);
        });

        // App startup
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
            app.MapIdentityApi<ApplicationUser>().RequireAuthorization("blog");

            app.MapGet("/identity", (HttpContext context) =>
                new JsonResult(context?.User?.Claims.Select(c => new { c.Type, c.Value }))
            );

            app.UseSession(); 
            await app.RunAsync();
        }
    }

}
