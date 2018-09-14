using System;
using Bognabot.Data.Exchange.Enums;

namespace Bognabot.Data.Exchange.Models
{
    public class TradeModel : ExchangeModel
    {
        public TradeType Side { get; set; }
        public long Size { get; set; }
        public double Price { get; set; }
    }
}
