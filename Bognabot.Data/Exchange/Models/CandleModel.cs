using System;
using Bognabot.Data.Exchange.Enums;

namespace Bognabot.Data.Exchange.Models
{
    public class CandleModel : ExchangeModel
    {
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public long Trades { get; set; }
        public string ExchangeName { get; set; }
        public TimePeriod Period { get; set; }
    }
}