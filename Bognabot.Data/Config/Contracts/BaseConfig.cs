using System.IO;
using System.Threading.Tasks;
using Bognabot.Storage.Core;
using Bognabot.Storage.Stores;
using Newtonsoft.Json;

namespace Bognabot.Data.Config.Contracts
{
    public abstract class BaseConfig<T> : IConfig where T : IUserConfig, new()
    {
        public string UserConfigFilename { get; set; }

        [JsonIgnore]
        public T UserConfig { get; private set; }

        public IUserConfig GetUserConfig()
        {
            return UserConfig;
        }

        public async Task LoadUserConfigAsync(string appDataPath)
        {
            var filePath = StorageUtils.PathCombine(appDataPath, UserConfigFilename);

            using (var js = new JsonStore<T>())
            {
                if (File.Exists(filePath))
                    UserConfig = await js.ReadAsync(filePath);
                else
                {
                    UserConfig = new T();
                    UserConfig.SetDefault();

                    await js.WriteAsync(filePath, UserConfig);
                }
            }
        }

        public async Task LoadEncryptedUserConfigAsync(string appDataPath, string key)
        {
            var filePath = StorageUtils.PathCombine(appDataPath, UserConfigFilename);

            using (var js = new EncryptedJsonStore<T>(key))
            {
                if (File.Exists(filePath))
                    UserConfig = await js.ReadAsync(filePath);
                else
                {
                    UserConfig = new T();
                    UserConfig.SetDefault();

                    await js.WriteAsync(filePath, UserConfig);
                }
            }
        }
    }
}