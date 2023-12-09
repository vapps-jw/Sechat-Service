using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OwaspHeaders.Core.Extensions;
using Sechat.Service.Configuration;
using Sechat.Service.Configuration.Installers;
using Sechat.Service.Hubs;
using Sechat.Service.Middleware;
using Sechat.Service.Utilities;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.EnvironmentName.Equals(AppConstants.CustomEnvironment.Test))
{
    _ = builder.Configuration.AddUserSecrets<Program>();
}

// Install Services
builder.InstallServices(typeof(IServiceInstaller).Assembly);

var app = builder.Build();

// Dev
if (app.Environment.IsDevelopment())
{
    _ = app.UseDeveloperExceptionPage();
}

// Prod
if (app.Environment.IsProduction())
{
    _ = app.UseExceptionHandler("/Error");
    _ = app.UseHsts();
}

app.UseCors(AppConstants.CorsPolicy.WebClient);
app.UseSecureHeadersMiddleware();
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseRateLimiter();

app.UseMiddleware<CustomResponseHeadersMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.MapHub<ChatHub>("/chat-hub");
app.MapHub<GamesHub>("/games-hub");

DBSetup.PrepareDatabase(app);
await UsersSetup.PrepareAdmins(app);
await GlobalSettingsSetup.InitializeGlobalSettings(app);

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName.Equals(AppConstants.CustomEnvironment.Test))
{
    await DBSetup.SeedData(app);
}

Console.WriteLine("--> Running App...");
app.Run();

