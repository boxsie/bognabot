using System.Collections.Generic;
using Bognabot.Data.Config.Contracts;
using Bognabot.Data.Exchange.Enums;

namespace Bognabot.Data.Config
{
    public class ExchangeConfig : BaseConfig<ExchangeUserConfig>
    {
        public string ExchangeName { get; set; }
        public string WebSocketUrl { get; set; }
        public string RestUrl { get; set; }
        public int AuthExpireSeconds { get; set; }
        public Dictionary<Instrument, string> SupportedInstruments { get; set; }
        public Dictionary<TimePeriod, string> SupportedTimePeriods { get; set; }
        public Dictionary<OrderType, string> SupportedOrderTypes { get; set; }
        public Dictionary<ExchangeChannel, string> SupportedRestChannels { get; set; }
        public Dictionary<ExchangeChannel, string> SupportedWebsocketChannels { get; set; }
    }
}