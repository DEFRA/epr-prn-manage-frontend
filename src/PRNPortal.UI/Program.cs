﻿using PRNPortal.Application.ConfigurationExtensions;
using PRNPortal.Application.Options;
using PRNPortal.UI.Extensions;
using PRNPortal.UI.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Logging;

using CookieOptions = PRNPortal.Application.Options.CookieOptions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var builderConfig = builder.Configuration;
var globalVariables = builderConfig.Get<GlobalVariables>();
string basePath = globalVariables.BasePath;

services.AddFeatureManagement();

services.AddAntiforgery(opts =>
{
    var cookieOptions = builderConfig.GetSection(CookieOptions.ConfigSection).Get<CookieOptions>();

    opts.Cookie.Name = cookieOptions.AntiForgeryCookieName;
    opts.Cookie.Path = basePath;
});

services
    .AddHttpContextAccessor()
    .RegisterWebComponents(builderConfig)
    .ConfigureMsalDistributedTokenOptions();

services
    .AddControllersWithViews(options =>
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    })
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

services.AddRazorPages();

services.Configure<ForwardedHeadersOptions>(options =>
{
    var forwardedHeadersOptions = builderConfig.GetSection("ForwardedHeaders").Get<ForwardedHeadersOptions>();

    options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
    options.ForwardedHostHeaderName = forwardedHeadersOptions.ForwardedHostHeaderName;
    options.OriginalHostHeaderName = forwardedHeadersOptions.OriginalHostHeaderName;
    options.AllowedHosts = forwardedHeadersOptions.AllowedHosts;
});

services.AddHealthChecks();

services.AddApplicationInsightsTelemetry();

services.AddHsts(options =>
{
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

services.AddAppHttpClient();

var app = builder.Build();

app.MapHealthChecks("/admin/health").AllowAnonymous();

app.UsePathBase(basePath);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseForwardedHeaders();

app.UseMiddleware<SecurityHeaderMiddleware>();
app.UseCookiePolicy();
app.UseStatusCodePagesWithReExecute("/error", "?statusCode={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserDataCheckerMiddleware>();
app.UseMiddleware<JourneyAccessCheckerMiddleware>();
app.UseMiddleware<AnalyticsCookieMiddleware>();

app.MapControllerRoute("default", "{controller=LandingController}/{action=Get}");

app.MapRazorPages();
app.MapControllers();
app.UseRequestLocalization();
app.Run();
