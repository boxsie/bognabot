using System.Threading.Tasks;

namespace Bognabot.Config.Core
{
    public interface IConfig
    {
        IUserConfig GetUserConfig();
        Task LoadUserConfigAsync(string appDataPath, string filename);
        Task LoadEncryptedUserConfigAsync(string appDataPath, string filename, string key);
    }
}