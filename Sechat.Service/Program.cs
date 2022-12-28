using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sechat.Service.Config;
using Sechat.Service.Configuration;
using Sechat.Service.Middleware;
using Sechat.Service.Settings;
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

var configuration = builder.Configuration;

// Data Related
builder.InstallServices(builder.Configuration, typeof(IServiceInstaller).Assembly);

// Options from Settings
builder.Services.Configure<CorsSettings>(configuration.GetSection(nameof(CorsSettings)));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

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

app.UseRouting();
app.UseCors(AppConstants.CorsPolicies.WebClient);
app.UseAuthentication();
app.UseAuthorization();

app.UseCustomResponseHeaders();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();

