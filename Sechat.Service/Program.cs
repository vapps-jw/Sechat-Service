using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Service.Config;
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
builder.Services.AddDbContext<SechatContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("Master"),
    serverAction =>
    {
        _ = serverAction.EnableRetryOnFailure(3);
        _ = serverAction.CommandTimeout(20);
    }));
builder.Services.AddDataProtection()
                .SetApplicationName("vapps")
                .PersistKeysToDbContext<SechatContext>();
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<SechatContext>()
                .AddDefaultTokenProviders();

// Options from Settings
builder.Services.Configure<CorsSettings>(configuration.GetSection(nameof(CorsSettings)));

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
