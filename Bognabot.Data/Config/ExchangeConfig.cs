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
        public Dictionary<Instrument, string> SupportedInstruments { get; set; }
        public Dictionary<TimePeriod, string> SupportedTimePeriods { get; set; }
        public string TradePath { get; set; }
        public string BookPath { get; set; }
        public string CandlePath { get; set; }
    }
}