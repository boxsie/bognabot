using System;

namespace Bognabot.Domain.Entities.Instruments
{
    public class Candle
    {
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public double Trades { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
