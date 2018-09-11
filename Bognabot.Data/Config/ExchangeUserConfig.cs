using Bognabot.Data.Config.Contracts;

namespace Bognabot.Data.Config
{
    public class ExchangeUserConfig : IUserConfig
    {
        public string Key { get; set; }
        public string Secret { get; set; }
        public int MaxDataPoints { get; set; }

        public void SetDefault()
        {
            MaxDataPoints = 350;
        }
    }
}