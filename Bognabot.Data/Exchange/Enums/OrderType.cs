using System.ComponentModel;

namespace Bognabot.Data.Exchange.Enums
{
    public enum OrderType
    {
        [DisplayName("Market")]
        Market,

        [DisplayName("Limit")]
        Limit,

        [DisplayName("Limit Market")]
        LimitMarket,

        [DisplayName("Stop")]
        Stop
    }
}