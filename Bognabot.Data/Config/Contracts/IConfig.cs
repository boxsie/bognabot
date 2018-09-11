using System.Threading.Tasks;

namespace Bognabot.Data.Config.Contracts
{
    public interface IConfig
    {
        IUserConfig GetUserConfig();
        Task LoadUserConfigAsync(string appDataPath);
        Task LoadEncryptedUserConfigAsync(string appDataPath, string key);
    }
}