using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Sechat.Service.Configuration;
using Sechat.Service.Configuration.Installers;
using Sechat.Service.Hubs;
using Sechat.Service.Middleware;
using Sechat.Service.Utilities;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging
if (builder.Environment.IsDevelopment())
{
    _ = builder.Host.UseSerilog((context, config) => { _ = config.WriteTo.Console(); });
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

app.UseCors(AppConstants.CorsPolicies.WebClient);
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CustomResponseHeadersMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.MapDefaultControllerRoute();
app.MapHub<ChatHub>("/chat-hub");

if (app.Environment.IsProduction())
{
    DbManager.PrepareDatabase(app);
}
if (app.Environment.IsDevelopment())
{
    DbManager.EnsureCreatedDatabase(app);
}
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName.Equals(AppConstants.CustomEnvironments.TestEnv))
{
    await DbManager.SeedData(app);
}

app.Run();

