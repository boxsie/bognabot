﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bognabot.Data.Config;
using Bognabot.Data.Config.Contracts;
using Bognabot.Storage.Core;
using Bognabot.Storage.Stores;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Bognabot.Data
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

            AppDataPath = StorageUtils.PathCombine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "Config");
            
            Logger.Log(LogLevel.Debug, $"Application data path set to {AppDataPath}");
        }

        public static void InitialiseConfig(IEnumerable<IConfig> appCfgs)
        {
            _configs = appCfgs.ToDictionary(x => x.GetType());

            UserDataPath = GetConfig<GeneralConfig>()?.UserConfig.UserDataPath ?? null;

            if (UserDataPath == null)
                Logger.Log(LogLevel.Warn, $"Could not find the Genral Config when trying to set the data path");

            Logger.Log(LogLevel.Debug, $"User data path set to {UserDataPath}");

            if (!Directory.Exists(UserDataPath))
            {
                Logger.Log(LogLevel.Debug, $"Application data directory not found, creating...");

                Directory.CreateDirectory(UserDataPath);
            }
        }

        public static T GetConfig<T>() where T : IConfig
        {
            var tType = typeof(T);

            if (_configs.ContainsKey(tType))
                return (T)_configs[tType];

            Logger.Log(LogLevel.Error, $"Unable to get {tType}");

            return default(T);
        }

        public static async Task<string> GetConfigJsonAsync(string configName)
        {
            using (var store = new TextStore())
            {
                var cfgName = configName.ToLower();
                
                var liveFile = StorageUtils.PathCombine(AppDataPath, $"{cfgName}{LiveFileExt}");

                if (!File.Exists(liveFile))
                {
                    Logger.Log(LogLevel.Fatal, $"{cfgName}{LiveFileExt} is missing from {AppDataPath}");
                    throw new FileNotFoundException();
                }

                var liveJson = await store.ReadAsync(liveFile);

                if (!IsDebug)
                    return liveJson;

                var debugFile = StorageUtils.PathCombine(AppDataPath, $"{cfgName}{DebugFileExt}");

                if (!File.Exists(debugFile))
                {
                    Logger.Log(LogLevel.Warn, $"{cfgName}{DebugFileExt} is missing from {AppDataPath}, using live config");
                    return liveJson;
                }

                var debugJson = await store.ReadAsync(debugFile);

                var jObjLive = JObject.Parse(liveJson);
                var jObjDebug = JObject.Parse(debugJson);

                foreach (var prop in jObjDebug.Properties())
                {
                    var targetProperty = jObjLive.Property(prop.Name);

                    if (targetProperty == null)
                        jObjDebug.Add(prop.Name, prop.Value);
                    else
                        targetProperty.Value = prop.Value;
                }

                return jObjLive.ToString(Formatting.None);
            }
        }
    }
}