using System;
using System.Runtime.InteropServices;
using Bognabot.Config.Core;
using Bognabot.Storage.Core;

namespace Bognabot.Config.Storage
{
    public class StorageUserConfig : UserConfig
    {
        public string UserDataPath { get; set; }
        
        public override void SetDefault()
        {
            UserDataPath = GetDefaultUserDataPath();
        }

        private static string GetDefaultUserDataPath()
        {
            var path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.GetEnvironmentVariable("LocalAppData")
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? $"~/Library/Application Support/"
                    : $"Home/";

            return StorageUtils.PathCombine(path, App.General.App.AppName, true);
        }
    }
}