using System;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Data.Models.Exchange
{
    public class BookModel
    {
        public Instrument Instrument { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TradeType Side { get; set; }
        public long Size { get; set; }
        public double Price { get; set; }
    }
}