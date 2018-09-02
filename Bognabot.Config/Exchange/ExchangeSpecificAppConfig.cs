namespace Bognabot.Config.Exchange
{
    public class ExchangeSpecificAppConfig
    {
        public string WebSocketUrl { get; set; }
        public string RestUrl { get; set; }
        public string[] SupportedInstruments { get; set; }
        public string[] InstrumentNames { get; set; }
        public string TradePath { get; set; }
        public string BookPath { get; set; }
        public string CandlePath { get; set; }
    }
}