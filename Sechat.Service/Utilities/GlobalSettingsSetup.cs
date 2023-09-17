using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Data;
using Sechat.Data.Models.GlobalModels;
using Sechat.Service.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sechat.Service.Utilities;

public static class GlobalSettingsSetup
{
    public static async Task InitializeGlobalSettings(IApplicationBuilder app)
    {
        Console.WriteLine("--> Checking Global Settings...");
        using var serviceScope = app.ApplicationServices.CreateScope();

        var context = serviceScope.ServiceProvider.GetService<SechatContext>();
        var settings = context.GlobalSettings.ToList();

        if (!settings.Any(s => s.Id.Equals(AppGlobalSettings.SettingName.RegistrationStatus)))
        {
            Console.WriteLine($"--> Adding Global Setting {AppGlobalSettings.SettingName.RegistrationStatus}");
            _ = context.Add(new GlobalSetting() { Id = AppGlobalSettings.SettingName.RegistrationStatus, Value = AppGlobalSettings.RegistrationStatus.Allowed });
        }

        _ = await context.SaveChangesAsync();
    }
}
