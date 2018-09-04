using Bognabot.Config.Core;

namespace Bognabot.Config.Exchange
{
    public class ExchangeUserConfig : IUserConfig
    {
        public string Filename => "exchange.json";
        public string EncryptionKey => "moop";

        public string Key { get; private set; }
        public string Secret { get; private set; }
        public int MaxDataPoints { get; private set; }

        public void SetDefault()
        {
            Key = "iiMc1cci1lNhyVpmrILHqCRY";
            Secret = "nkj7IJuPVugn8kYmIqXQ093ho7Z8ccipeNbn6RTZrosnZCUh";
            MaxDataPoints = 350;
        }
    }
}