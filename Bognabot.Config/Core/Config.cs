using System.IO;
using System.Threading.Tasks;
using Bognabot.Storage.Core;
using Bognabot.Storage.Stores;

namespace Bognabot.Config.Core
{
    public class Config<T, TY> where T : AppData where TY : UserData, new()
    {
        public T App { get; set; }
        public TY User { get; set; }

        public Config(T appData)
        {
            App = appData;
        }

        public async Task LoadUserSettingsAsync(string appDataPath)
        {
            var filePath = StorageUtils.PathCombine(appDataPath, App.Filename);
            
            using (var js = new JsonStore<TY>())
            {
                if (File.Exists(filePath))
                    User = await js.ReadAsync(filePath);
                else
                {
                    User = new TY();
                    User.SetDefault();
                    await js.WriteAsync(filePath, User);
                }
            }
        }
    }
}