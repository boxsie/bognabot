using Bognabot.Config.Core;

namespace Bognabot.Config.General
{
    public class GeneralConfig : BaseConfig<GeneralUserConfig>
    {
        public string AppName { get; set; }
        public string DbFilename { get; set; }
    }
}