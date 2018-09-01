using System;

namespace Bognabot.Domain.Entities.Instruments
{
    public class InstrumentCandle
    {
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        
        public DateTime CreatedOn { get; set; }
    }
}
