using System.Collections.Generic;
using Bognabot.Config.Enums;
using Bognabot.Config.Exchange;

namespace Bognabot.Config.Core
{
    public class ExchangeConfig : BaseConfig<ExchangeUserConfig>
    {
        public SupportedExchange Exchange { get; set; }
        public string WebSocketUrl { get; set; }
        public string RestUrl { get; set; }
        public Dictionary<Instrument, string> SupportedInstruments { get; set; }
        public Dictionary<TimePeriod, string> SupportedTimePeriods { get; set; }
        public string TradePath { get; set; }
        public string BookPath { get; set; }
        public string CandlePath { get; set; }
    }
}