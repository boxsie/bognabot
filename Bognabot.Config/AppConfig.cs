using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Bognabot.Config.Api;
using Bognabot.Config.Core;
using Bognabot.Config.General;
using Bognabot.Config.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bognabot.Config
{
    public static class AppConfig
    {
        public static Config<GeneralApp, GeneralUser> General { get; private set; }
        public static Config<StorageApp, StorageUser> Storage { get; private set; }
        public static Config<ApiApp, ApiUser> Api { get; private set; }

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

            services.Configure<GeneralApp>(config.GetSection("General"));
            services.Configure<StorageApp>(config.GetSection("Storage"));
            services.Configure<ApiApp>(config.GetSection("Api"));

            return config;
        }

        public static async Task AttachConfig(IHostingEnvironment hostingEnvironment, IServiceProvider provider)
        {
            var appDataPath = $"{hostingEnvironment.ContentRootPath}/App_Data/";

            General = GetConfig<GeneralApp, GeneralUser>(provider);
            Storage = GetConfig<StorageApp, StorageUser>(provider);
            Api = GetConfig<ApiApp, ApiUser>(provider);

            await Storage.LoadUserSettingsAsync(appDataPath);
            await General.LoadUserSettingsAsync(appDataPath);
            await Api.LoadEncryptedUserSettingsAsync(appDataPath, "moop");
        }

        private static Config<T, TY> GetConfig<T, TY>(IServiceProvider provider) where T : AppData, new() where TY : UserData, new()
        {
            return new Config<T, TY>(provider.GetRequiredService<IOptions<T>>().Value);
        }
    }
}
