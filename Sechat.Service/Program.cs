using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

app.UseCors(AppConstants.CorsPolicies.WebClient);
//app.UseSecureHeadersMiddleware();
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CustomResponseHeadersMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.MapDefaultControllerRoute();
app.MapHub<ChatHub>("/chat-hub");

DbManager.PrepareDatabase(app);

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName.Equals(AppConstants.CustomEnvironments.TestEnv))
{
    await DbManager.SeedData(app);
}

Console.WriteLine("--> Running App...");
app.Run();

