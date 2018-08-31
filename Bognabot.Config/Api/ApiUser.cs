using Bognabot.Config.Core;

namespace Bognabot.Config.Api
{
    public class ApiUser : UserData
    {
        public string BitmexKey { get; set; }
        public string BitmexSecret { get; set; }

        public override void SetDefault()
        {
            BitmexKey = "iiMc1cci1lNhyVpmrILHqCRY";
            BitmexSecret = "nkj7IJuPVugn8kYmIqXQ093ho7Z8ccipeNbn6RTZrosnZCUh";
        }
    }
}