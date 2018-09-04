using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bognabot.Config.Core;
using Bognabot.Config.Enums;
using Bognabot.Config.General;
using Bognabot.Config.Storage;
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
        private static Dictionary<SupportedExchange, ExchangeConfig> _exchangeConfigs;

        static Cfg()
        {
#if DEBUG
            IsDebug = true;
#else
            IsDebug = false;
#endif
            Logger = NLog.LogManager.GetCurrentClassLogger();

            _configs = new Dictionary<Type, IConfig>();
            _exchangeConfigs = new Dictionary<SupportedExchange, ExchangeConfig>();
        }

        public static void AddServices(IServiceCollection services)
        {
            AppDataPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Logger.Log(LogLevel.Debug, $"Application data path set to {AppDataPath}");

            services.AddSingleton<GeneralConfig>((x) => ConfigFactory<GeneralConfig>().GetAwaiter().GetResult());
            services.AddSingleton<StorageConfig>((x) => ConfigFactory<StorageConfig>().GetAwaiter().GetResult());
            services.AddSingleton<IEnumerable<ExchangeConfig>>((x) => ConfigCollectionFactory<ExchangeConfig>().GetAwaiter().GetResult());
        }

        public static async Task LoadUserDataAsync(IServiceProvider serviceProvider, string appRootPath)
        {
            _configs = serviceProvider.GetServices<IConfig>().ToDictionary(x => x.GetType());
            _exchangeConfigs = serviceProvider.GetService<IEnumerable<ExchangeConfig>>().ToDictionary(x => x.Exchange);

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

        public static ExchangeConfig GetExchangeConfig(SupportedExchange exchange)
        {
            if (_exchangeConfigs.ContainsKey(exchange))
                return _exchangeConfigs[exchange];

            Logger.Log(LogLevel.Error, $"Unable to get {exchange}");

            return null;
        }

        public static IEnumerable<ExchangeConfig> GetExchangeConfigs()
        {
            return _exchangeConfigs.Values;
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

        private static async Task<IEnumerable<T>> ConfigCollectionFactory<T>() where T : IConfig
        {
            var configName = typeof(T).Name.Replace("Config", "");

            try
            {
                var jsonCol = await GetConfigCollectionJson(configName);

                var configs = jsonCol.Select(JsonConvert.DeserializeObject<T>);

                Logger.Log(LogLevel.Info, $"{configName} application data load complete");

                return configs;
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
                var pathBase = $"{GetConfigBasePath(configName)}{configName.ToLower()}";

                var pathFileExt = IsDebug && File.Exists($"{pathBase}{DebugFileExt}") ? DebugFileExt : LiveFileExt;

                var json = await store.ReadAsync($"{pathBase}{pathFileExt}");

                return json;
            }
        }

        private static async Task<List<string>> GetConfigCollectionJson(string configName)
        {
            using (var store = new TextStore())
            {
                var jsonCol = new List<string>();

                var configFiles = Directory.GetFiles(GetConfigBasePath(configName))
                    .Where(x => IsDebug && x.Contains(DebugFileExt) || x.Contains(LiveFileExt));

                foreach (var configFile in configFiles)
                    jsonCol.Add(await store.ReadAsync(configFile));

                return jsonCol;
            }
        }

        private static string GetConfigBasePath(string configName)
        {
            return $"{AppDataPath}\\{configName}\\";
        }
    }
}
