using System.IO;
using System.Threading.Tasks;
using Bognabot.Storage.Core;
using Bognabot.Storage.Stores;

namespace Bognabot.Config.Core
{
    public class Config<T, TY> where T : AppConfig where TY : UserConfig, new()
    {
        public T App { get; private set; }
        public TY User { get; private set; }

        public Config(T appData)
        {
            App = appData;
        }

        public async Task LoadUserDataAsync(string appDataPath, string key = null)
        {
            User = key == null
                ? await LoadUserSettingsAsync(appDataPath, App.Filename)
                : await LoadEncryptedUserSettingsAsync(appDataPath, App.Filename, key);
        }

        private static async Task<TY> LoadUserSettingsAsync(string appDataPath, string filename)
        {
            var filePath = StorageUtils.PathCombine(appDataPath, filename);

            using (var js = new JsonStore<TY>())
            {
                var settings = default(TY);

                if (File.Exists(filePath))
                    settings = await js.ReadAsync(filePath);
                else
                {
                    settings = new TY();
                    settings.SetDefault();

                    await js.WriteAsync(filePath, settings);
                }

                return settings;
            }
        }

        private static async Task<TY> LoadEncryptedUserSettingsAsync(string appDataPath, string filename, string key)
        {
            var filePath = StorageUtils.PathCombine(appDataPath, filename);

            using (var js = new EncryptedJsonStore<TY>(key))
            {
                var settings = default(TY);

                if (File.Exists(filePath))
                    settings = await js.ReadAsync(filePath);
                else
                {
                    settings = new TY();
                    settings.SetDefault();

                    await js.WriteAsync(filePath, settings);
                }

                return settings;
            }
        }
    }
}