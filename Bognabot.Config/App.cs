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
    public static class App
    {
        public static Config<GeneralAppConfig, GeneralUserConfig> General { get; private set; }
        public static Config<StorageAppConfig, StorageUserConfig> Storage { get; private set; }
        public static Config<ExchangeAppConfig, ExchangeUserConfig> Exchange { get; private set; }

        public static IConfigurationRoot BuildConfig(IServiceCollection services)
        {
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

        public static async Task AttachConfig(string appRootPath, IServiceProvider provider)
        {
            var appDataPath = $"{appRootPath}/App_Data/";

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            General = GetConfig<GeneralAppConfig, GeneralUserConfig>(provider);
            Storage = GetConfig<StorageAppConfig, StorageUserConfig>(provider);
            Exchange = GetConfig<ExchangeAppConfig, ExchangeUserConfig>(provider);

            await Storage.LoadUserDataAsync(appDataPath);
            await General.LoadUserDataAsync(appDataPath);
            await Exchange.LoadUserDataAsync(appDataPath, "moop");
        }

        private static Config<T, TY> GetConfig<T, TY>(IServiceProvider provider) where T : Core.AppConfig, new() where TY : UserConfig, new()
        {
            return new Config<T, TY>(provider.GetRequiredService<IOptions<T>>().Value);
        }
    }
}
