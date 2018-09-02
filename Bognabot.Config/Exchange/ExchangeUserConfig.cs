using Bognabot.Config.Core;
using Bognabot.Storage.Core;

namespace Bognabot.Config.Exchange
{
    public class ExchangeUserConfig : UserConfig
    {
        public int HistoryDays { get; set; }

        public ExchangeSpecificUserConfig Bitmex { get; set; }

        public override void SetDefault()
        {
            HistoryDays = 7;

            Bitmex = new ExchangeSpecificUserConfig
            {
                Key = "iiMc1cci1lNhyVpmrILHqCRY",
                Secret = "nkj7IJuPVugn8kYmIqXQ093ho7Z8ccipeNbn6RTZrosnZCUh"
            };
        }
    }
}