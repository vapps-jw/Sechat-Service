using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sechat.Service.Config;
using Sechat.Service.Configuration;
using Sechat.Service.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Secrets Injection
if (builder.Environment.IsProduction())
{
    _ = builder.Configuration.AddJsonFile(AppConstants.Paths.SecretSettings, true, true);
}

// Kestrel Settings
if (builder.Environment.IsDevelopment())
{
    _ = builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(5000));
}

// Logging
if (builder.Environment.IsDevelopment())
{
    _ = builder.Host.UseSerilog((context, config) => { _ = config.WriteTo.Console(); });
}

// Install Services
builder.InstallServices(builder.Configuration, typeof(IServiceInstaller).Assembly);

// Setup Options from Settings
builder.Services.AddConfig(builder.Configuration);

builder.Services.AddSignalR(options =>
{
    options.DisableImplicitFromServicesParameters = true;
});
builder.Services.AddControllers();

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

app.UseHttpsRedirection();

app.UseCors(AppConstants.CorsPolicies.WebClient);
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCustomResponseHeaders();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapDefaultControllerRoute();
});

app.Run();

