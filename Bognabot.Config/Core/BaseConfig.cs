using System.IO;
using System.Threading.Tasks;
using Bognabot.Storage.Core;
using Bognabot.Storage.Stores;
using Newtonsoft.Json;

namespace Bognabot.Config.Core
{
    public abstract class BaseConfig<T> : IConfig where T : IUserConfig, new()
    {
        [JsonIgnore]
        public T UserConfig { get; private set; }

        public IUserConfig GetUserConfig()
        {
            return UserConfig;
        }

        public async Task LoadUserConfigAsync(string appDataPath, string filename)
        {
            var filePath = StorageUtils.PathCombine(appDataPath, filename);

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

        public async Task LoadEncryptedUserConfigAsync(string appDataPath, string filename, string key)
        {
            var filePath = StorageUtils.PathCombine(appDataPath, filename);

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