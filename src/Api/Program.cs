/*
 Copyright (c) 2024 HigginsSoft, Alexander Higgins - https://github.com/alexhiggins732/ 

 Copyright (c) 2018, Brock Allen & Dominick Baier. All rights reserved.

 Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information. 
 Source code and license this software can be found 

 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.
*/

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Interface;
using Yavsc.Models;
using Yavsc.Services;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.Title = "API";

        var builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;

        // accepts any access token issued by identity server
        // adds an authorization policy for scope 'api1'
        services
            .AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy
                        .RequireAuthenticatedUser()
                        .RequireClaim("scope", "scope2");
                });
            })
            .AddCors(options =>
            {
                // this defines a CORS policy called "default"
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins("https://localhost:5003")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            })
            .AddControllersWithViews();

        // accepts any access token issued by identity server
        var authenticationBuilder = services.AddAuthentication()
                 .AddJwtBearer("Bearer", options =>
                 {
                     options.IncludeErrorDetails = true;
                     options.Authority = "https://localhost:5001";
                     options.TokenValidationParameters =
                         new() { ValidateAudience = false };
                 });
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
        services.AddScoped<UserManager<ApplicationUser>>();
        services.AddSingleton<IConnexionManager, HubConnectionManager>();
        services.AddSingleton<ILiveProcessor, LiveProcessor>();
        services.AddTransient<IFileSystemAuthManager, FileSystemAuthManager>();
        services.AddIdentityApiEndpoints<ApplicationUser>();
        builder.Services.AddSession();
        
        services.AddTransient<ITrueEmailSender, MailSender>()
        .AddTransient<IBillingService, BillingService>()
        .AddTransient<ICalendarManager, CalendarManager>()
        .AddTransient<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>()
        .AddTransient<DbContext>();

        using (var app = builder.Build())
        {
            if (app.Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization().UseCors("default")
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapDefaultControllerRoute()
                        .RequireAuthorization();
                });
            app.MapIdentityApi<ApplicationUser>().RequireAuthorization("ApiScope");
            app.UseSession();
            await app.RunAsync();
        };




    }
}
