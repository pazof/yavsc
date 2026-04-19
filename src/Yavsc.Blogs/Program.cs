/*
 Copyright (c) 2024 HigginsSoft, Alexander Higgins - https://github.com/alexhiggins732/ 

 Copyright (c) 2018, Brock Allen & Dominick Baier. All rights reserved.

 Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information. 
 Source code and license this software can be found 

 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.
*/

using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc;
using Yavsc.Helpers;
using Yavsc.Interface;
using Yavsc.Models;
using Yavsc.Services;
using Yavsc.Server.Helpers;

internal class Program
{
    private static async Task Main(string[] args)
  {
    Console.Title = "Yavsc.Blogs";

    var builder = WebApplication.CreateBuilder(args);

    var services = builder.Services;

    var authority = builder.GetAuthority();
    var audience = builder.GetAudience();

    // builder.Services.AddDistributedMemoryCache();

    // accepts any access token issued by identity server
    // adds an authorization policy for scope 'scope1'

    services
        .AddAuthorization(options =>
        {
          options.AddPolicy("ApiScope", policy =>
          {
            policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(JwtClaimTypes.Scope, new string[] { "blogs" });
          });
        })
        .AddCors(options =>
        {
          // this defines a CORS policy called "default"
          options.AddPolicy("default", policy =>
          {
            policy.WithOrigins(audience)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
          });
        })
        .AddControllers();

    // accepts any access token issued by identity server
    var authenticationBuilder = services.AddAuthentication("Bearer")
             .AddJwtBearer("Bearer", options =>
             {
               options.IncludeErrorDetails = true;
               options.Authority = authority;
               options.TokenValidationParameters =
                       new() { ValidateAudience = false, RoleClaimType = Constants.RoleClaimType };
               options.MapInboundClaims = true;
             });
             
    services.AddDbContext<ApplicationDbContext>(options =>
       options.UseNpgsql(builder.Configuration.GetConnectionString(Constants.YavscConnectionStringName)));

    services.AddTransient<ITrueEmailSender, MailSender>()
       .AddTransient<IBillingService, BillingService>()
       .AddTransient<ICalendarManager, CalendarManager>();
    services.AddTransient<IFileSystemAuthManager, FileSystemAuthManager>();

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
          /*         .UseEndpoints(endpoints =>
                   {
                       endpoints.MapDefaultControllerRoute()
                           .RequireAuthorization();
                   })*/

          ;
      //   app.MapIdentityApi<ApplicationUser>().RequireAuthorization("ApiScope"); 
      app.MapDefaultControllerRoute();
      app.MapGet("/identity", (HttpContext context) =>
          new JsonResult(context?.User?.Claims.Select(c => new { c.Type, c.Value }))
      );

      //     app.UseSession(); 
      await app.RunAsync();
    }
  }

}
