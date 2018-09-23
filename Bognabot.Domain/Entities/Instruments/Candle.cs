using System;
using System.Runtime.Serialization;

namespace Bognabot.Domain.Entities.Instruments
{
    public class Candle : IEntity
    {
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public double Trades { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
