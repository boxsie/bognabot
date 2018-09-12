using System;
using System.Collections.Generic;
using System.Text;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Domain.Entities.Account
{
    public class Trade : IEntity
    {
        public int TradeId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
