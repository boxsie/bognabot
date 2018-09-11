using Bognabot.Data.Config.Contracts;

namespace Bognabot.Data.Config
{
    public class GeneralConfig : BaseConfig<GeneralUserConfig>
    {
        public string AppName { get; set; }
        public string DbFilename { get; set; }
    }
}