using Newtonsoft.Json;

namespace Bognabot.Config.Core
{
    public interface IUserConfig
    {
        [JsonIgnore]
        string Filename { get; }

        [JsonIgnore]
        string EncryptionKey { get; }

        void SetDefault();
    }
}