using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bognabot.Config;
using Bognabot.Config.Models;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange.Contracts;
using Bognabot.Storage.Core;
using Bognabot.Storage.Stores;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NLog;

namespace Bognabot.Config
{
    public static class Cfg
    {
        public static readonly ILogger Logger;
        public static bool IsDebug { get; private set; }
        public static string UserDataPath { get; private set; }
        public static string AppDataPath { get; private set; }  
        
        private const string DebugFileExt = ".debug.json";
        private const string LiveFileExt = ".live.json";

        private static Dictionary<Type, IConfig> _configs;

        static Cfg()
        {
#if DEBUG
            IsDebug = true;
#else
            IsDebug = false;
#endif
            Logger = NLog.LogManager.GetCurrentClassLogger();

            _configs = new Dictionary<Type, IConfig>();
        }

        public static void AddServices(IServiceCollection services, IEnumerable<Type> exchangeServiceTypes)
        {
            AppDataPath = StorageUtils.PathCombine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "Config");

            Logger.Log(LogLevel.Debug, $"Application data path set to {AppDataPath}");
            
            Logger.Log(LogLevel.Info, "Checking exchange services...");

            var contractType = typeof(IExchangeService);

            foreach (var exchType in exchangeServiceTypes)
            {
                var exchName = exchType.Name.Replace("Service", "");

                if (!contractType.IsAssignableFrom(exchType))
                    Logger.Log(LogLevel.Warn, $"{exchType} exchange service does not derive from {contractType} and will not be loaded");

                services.AddSingleton(contractType, service => ExchangeServiceFactory(service, exchType, exchName));

                Logger.Log(LogLevel.Info, $"Found {exchName} exchange");
            }
            
            services.AddSingleton<IConfig, GeneralConfig>((x) => ConfigFactory<GeneralConfig>().GetAwaiter().GetResult());
            services.AddSingleton<IConfig, StorageConfig>((x) => ConfigFactory<StorageConfig>().GetAwaiter().GetResult());
        }

        public static async Task LoadUserDataAsync(IEnumerable<IConfig> appConfigs, string appRootPath)
        {
            _configs = appConfigs.ToDictionary(x => x.GetType());

            Logger.Log(LogLevel.Info, "Loading user data...");

            UserDataPath = "moop";

            Logger.Log(LogLevel.Debug, $"User data path set to {UserDataPath}");

            if (!Directory.Exists(UserDataPath))
            {
                Logger.Log(LogLevel.Debug, $"Application data directory not found, creating...");

                Directory.CreateDirectory(UserDataPath);
            }

            foreach (var config in _configs.Values)
            {
                var userConfig = config.GetUserConfig();

                if (string.IsNullOrEmpty(userConfig.EncryptionKey))
                    await config.LoadUserConfigAsync(AppDataPath, userConfig.Filename);
                else
                    await config.LoadEncryptedUserConfigAsync(AppDataPath, userConfig.Filename, userConfig.EncryptionKey);
            }

            Logger.Log(LogLevel.Info, "User data load complete");
        }

        public static T GetConfig<T>() where T : IConfig
        {
            var tType = typeof(T);

            if (_configs.ContainsKey(tType))
                return (T)_configs[tType];

            Logger.Log(LogLevel.Error, $"Unable to get {tType}");

            return default(T);
        }

        private static object ExchangeServiceFactory(IServiceProvider service, Type exchType, string exchName)
        {
            var cfg = ConfigFactory<ExchangeConfig>(exchName.ToLower()).GetAwaiter().GetResult();

            if (cfg == null || !string.Equals(cfg.ExchangeName, exchName, StringComparison.CurrentCultureIgnoreCase))
                Logger.Log(LogLevel.Warn, $"Cannot locate the config for {exchType.Name}");

            return Activator.CreateInstance(exchType, new object[] { service.GetService<ILogger>(), cfg });
        }

        private static async Task<T> ConfigFactory<T>(string name = null) where T : IConfig
        {
            var configName = name ?? typeof(T).Name.Replace("Config", "");

            Logger.Log(LogLevel.Info, $"Loading {configName} application data...");

            try
            {
                var config = JsonConvert.DeserializeObject<T>(await GetConfigJson(configName));

                Logger.Log(LogLevel.Info, $"{configName} application data load complete");

                return config;
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Fatal, $"Unable to load {configName} application data");
                Logger.Log(LogLevel.Fatal, e.Message);
                Logger.Log(LogLevel.Fatal, e.StackTrace);

                throw;
            }
        }

        private static async Task<string> GetConfigJson(string configName)
        {
            using (var store = new TextStore())
            {
                var cfgName = configName.ToLower();

                var pathFileExt = IsDebug && File.Exists(StorageUtils.PathCombine(AppDataPath, $"{cfgName}{DebugFileExt}")) ? DebugFileExt : LiveFileExt;

                var json = await store.ReadAsync(StorageUtils.PathCombine(AppDataPath, $"{cfgName}{pathFileExt}"));

                return json;
            }
        }
    }
}
