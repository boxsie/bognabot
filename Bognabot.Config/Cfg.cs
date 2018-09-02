using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Bognabot.Config.Core;
using Bognabot.Config.Exchange;
using Bognabot.Config.General;
using Bognabot.Config.Storage;
using Bognabot.Storage.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bognabot.Config
{
    public static class Cfg
    {
        public static string AppDataPath { get; private set; }
        public static Config<GeneralAppConfig, GeneralUserConfig> General { get; private set; }
        public static Config<StorageAppConfig, StorageUserConfig> Storage { get; private set; }
        public static Config<ExchangeAppConfig, ExchangeUserConfig> Exchange { get; private set; }

        public static IConfigurationRoot BuildConfig(IServiceCollection services, string appRootPath)
        {
            AppDataPath = $"{appRootPath}/App_Data/";
            
            if (!Directory.Exists(AppDataPath))
                Directory.CreateDirectory(AppDataPath);

#if DEBUG
            const string appSettingsFile = "config.debug.json";
#else
            const string appSettingsFile = "config.json";
#endif
            var config = new ConfigurationBuilder()
                .AddJsonFile(appSettingsFile, false)
                .Build();

            services.Configure<GeneralAppConfig>(config.GetSection("General"));
            services.Configure<StorageAppConfig>(config.GetSection("Storage"));
            services.Configure<ExchangeAppConfig>(config.GetSection("Exchange"));

            return config;
        }

        public static void AttachConfig(IServiceProvider provider)
        {
            General = GetConfig<GeneralAppConfig, GeneralUserConfig>(provider);
            Storage = GetConfig<StorageAppConfig, StorageUserConfig>(provider);
            Exchange = GetConfig<ExchangeAppConfig, ExchangeUserConfig>(provider);
        }

        public static async Task LoadUserDataAsync()
        {
            await Storage.LoadUserDataAsync();
            await General.LoadUserDataAsync();
            await Exchange.LoadUserDataAsync("moop");
        }

        private static Config<T, TY> GetConfig<T, TY>(IServiceProvider provider) where T : Core.AppConfig, new() where TY : UserConfig, new()
        {
            return new Config<T, TY>(provider.GetRequiredService<IOptions<T>>().Value);
        }
    }
}
