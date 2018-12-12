namespace Bognabot.Data.Exchange.Dtos
{
    public class PositionDto : ExchangeDto
    {
        public double? Quantity { get; set; }
        public double? EntryPrice { get; set; }
    }
}