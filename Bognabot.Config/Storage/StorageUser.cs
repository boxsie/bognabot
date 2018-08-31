using System;
using System.Runtime.InteropServices;
using Bognabot.Config.Core;
using Bognabot.Storage.Core;

namespace Bognabot.Config.Storage
{
    public class StorageUser : UserData
    {
        public string UserDataPath { get; set; }

        public StorageUser()
        {
            UserDataPath = GetDefaultUserDataPath();
        }
        
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

            return StorageUtils.PathCombine(path, AppConfig.General.App.AppName, true);
        }
    }
}