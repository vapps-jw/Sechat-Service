using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sechat.Data;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos.AutoMapperProfiles;
using Sechat.Service.Services;
using System;

namespace Sechat.Service.Configuration.Installers;

public class DataServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {

        // todo: check usecases
        //_ = webApplicationBuilder.Services.AddMarten(opts =>
        //{
        //    opts.Connection(webApplicationBuilder.Configuration.GetConnectionString("DocumentStore"));

        //    opts.AutoCreateSchemaObjects = webApplicationBuilder.Environment.IsProduction() ? AutoCreate.CreateOrUpdate : AutoCreate.All;
        //    opts.UseDefaultSerialization(
        //        serializerType: SerializerType.SystemTextJson,
        //        casing: Casing.CamelCase
        //    );

        //    opts.CreateDatabasesForTenants(c =>
        //    {
        //        _ = c.MaintenanceDatabase(webApplicationBuilder.Configuration.GetConnectionString("DocumentMaintenance"));
        //        _ = c.ForTenant()
        //            .CheckAgainstPgDatabase()
        //            .WithOwner("postgres")
        //            .WithEncoding("UTF-8")
        //            .ConnectionLimit(-1)
        //            .OnDatabaseCreated(_ =>
        //            {

        //            });
        //    });
        //    _ = opts.Schema.For<CalendarDocument>();
        //})
        //.ApplyAllDatabaseChangesOnStartup()
        //.AssertDatabaseMatchesConfigurationOnStartup();

        _ = webApplicationBuilder.Environment.EnvironmentName.Equals(AppConstants.CustomEnvironment.Test)
            ? webApplicationBuilder.Services.AddDbContextFactory<SechatContext>(options =>
                 options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            : webApplicationBuilder.Services.AddDbContextFactory<SechatContext>(options =>
                options.UseNpgsql(webApplicationBuilder.Configuration.GetConnectionString("Master"),
                    serverAction =>
                    {
                        _ = serverAction.EnableRetryOnFailure(3);
                        _ = serverAction.CommandTimeout(20);
                    })
                .ConfigureWarnings(c => c.Log((RelationalEventId.TransactionError, LogLevel.Error)))
                .ConfigureWarnings(c => c.Log((RelationalEventId.ConnectionError, LogLevel.Error)))
                .ConfigureWarnings(c => c.Log((RelationalEventId.MigrationsNotFound, LogLevel.Error)))
                .ConfigureWarnings(c => c.Log((RelationalEventId.MigrationsNotApplied, LogLevel.Error)))
                .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuted, LogLevel.Debug)))
                .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug))));

        _ = webApplicationBuilder.Services.AddScoped<ChatRepository>();
        _ = webApplicationBuilder.Services.AddScoped<UserRepository>();
        _ = webApplicationBuilder.Services.AddScoped<CalendarRepository>();

        _ = webApplicationBuilder.Services.AddTransient<CryptographyService>();
        _ = webApplicationBuilder.Services.AddTransient<UserDataService>();
        _ = webApplicationBuilder.Services.AddTransient<TokenService>();

        _ = webApplicationBuilder.Services.AddAutoMapper(
            typeof(DefaultProfile),
            typeof(ChatModelsProfile),
            typeof(CalendarModelsProfile));

        _ = webApplicationBuilder.Services.AddTransient<ContactSuggestionsService>();
    }
}
