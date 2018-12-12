using System;
using Bognabot.Data.Exchange.Enums;

namespace Bognabot.Data.Exchange.Dtos
{
    public class TradeDto : ExchangeDto
    {
        public TradeSide Side { get; set; }
        public long Size { get; set; }
        public double Price { get; set; }
    }
}
