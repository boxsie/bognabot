using System;
using System.Runtime.InteropServices;
using Bognabot.Config.Core;
using Bognabot.Config.General;
using Bognabot.Storage.Core;

namespace Bognabot.Config.Storage
{
    public class StorageUserConfig : IUserConfig
    {
        public string Filename => "storage.json";
        public string EncryptionKey => null;

        public string UserDataPath { get; set; }

        public void SetDefault()
        {
            UserDataPath = GetDefaultUserDataPath();
        }

        private static string GetDefaultUserDataPath()
        {
            return StorageUtils.GetDefaultUserDataPath(Cfg.GetConfig<GeneralConfig>().AppName);
        }
    }
}