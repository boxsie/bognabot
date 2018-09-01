using Bognabot.Config.Core;
using Bognabot.Storage.Core;

namespace Bognabot.Config.Exchange
{
    public class ExchangeUserConfig : UserConfig
    {
        public ExchangeSpecificUserConfig Bitmex { get; set; }

        public override void SetDefault()
        {

        }
    }
}