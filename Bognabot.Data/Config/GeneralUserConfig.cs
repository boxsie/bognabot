using Bognabot.Data.Config.Contracts;
using Bognabot.Storage.Core;

namespace Bognabot.Data.Config
{
    public class GeneralUserConfig : IUserConfig
    {
        public string UserDataPath { get; set; }

        public void SetDefault()
        {
            UserDataPath = GetDefaultUserDataPath();
        }

        private static string GetDefaultUserDataPath()
        {
            return StorageUtils.GetDefaultUserDataPath();
        }
    }
}