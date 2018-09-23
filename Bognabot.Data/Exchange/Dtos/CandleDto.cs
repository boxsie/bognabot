using Bognabot.Data.Exchange.Enums;

namespace Bognabot.Data.Exchange.Dtos
{
    public class CandleDto : ExchangeDto
    {
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public long Trades { get; set; }
        public TimePeriod Period { get; set; }
    }
}