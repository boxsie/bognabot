using System;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Data.Models.Exchange
{
    public class CandleModel
    {
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public long Trades { get; set; }
        public string ExchangeName { get; set; }
        public Instrument Instrument { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TimePeriod Period { get; set; }
    }
}