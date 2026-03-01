/*
 Copyright (c) 2024 HigginsSoft, Alexander Higgins - https://github.com/alexhiggins732/ 

 Copyright (c) 2018, Brock Allen & Dominick Baier. All rights reserved.

 Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information. 
 Source code and license this software can be found 

 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.
*/

using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;

JwtSecurityTokenHandler.DefaultMapInboundClaims = true;

var builder = WebApplication.CreateBuilder(args);

var authSection = builder.Configuration.GetSection("Authentication");

 var issuer = authSection.GetValue<String>("Issuer");
var clientSection = authSection.GetSection("Client");
var clientId = clientSection.GetValue<String>("Id");
var clientSecret = clientSection.GetValue<String>("Secret");
   
   
builder.Services.AddControllersWithViews();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = Constants.INTERNAL_SCHEME;
        options.DefaultChallengeScheme = Constants.EXTERNAL_SCHEME;
    })
    .AddCookie(Constants.INTERNAL_SCHEME)
    .AddOpenIdConnect(Constants.EXTERNAL_SCHEME, options =>
    {
        options.Authority = issuer;
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.ResponseType = Constants.AUTHENTICATION_RESPONSE_TYPE;

        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("scope2");
        options.MapInboundClaims = true;
        options.ClaimActions.MapUniqueJsonKey("preferred_username", "preferred_username");
        options.ClaimActions.MapUniqueJsonKey("gender", "gender");
        options.SaveTokens = true;
    });

using (var app = builder.Build())
{
    if (app.Environment.IsDevelopment())
        app.UseDeveloperExceptionPage();
    else
        app.UseExceptionHandler("/Home/Error");

    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapDefaultControllerRoute().RequireAuthorization();

    await app.RunAsync();
}
