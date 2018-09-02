using System;
using System.Collections.Generic;
using System.Text;

namespace Bognabot.Domain.Entities.Account
{
    public class Trade
    {
        public int TradeId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
