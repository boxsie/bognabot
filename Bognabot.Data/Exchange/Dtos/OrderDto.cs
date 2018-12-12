using Bognabot.Data.Exchange.Enums;

namespace Bognabot.Data.Exchange.Dtos
{
    public class OrderDto : ExchangeDto
    {
        public string OrderId { get; set; }
        public TradeSide Side { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
    }
}