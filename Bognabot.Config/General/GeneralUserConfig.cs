using Bognabot.Config.Core;

namespace Bognabot.Config.General
{
    public class GeneralUserConfig : IUserConfig
    {
        public string Filename => "general.json";
        public string EncryptionKey => null;

        public void SetDefault()
        {

        }
    }
}