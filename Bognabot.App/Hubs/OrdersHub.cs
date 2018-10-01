using System.Threading.Channels;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Services.Exchange;
using Microsoft.AspNetCore.SignalR;

namespace Bognabot.App.Hubs
{
    public class OrdersHub : Hub
    {
        private readonly OrdersHubControl _hubControl;

        public OrdersHub(OrdersHubControl hubControl)
        {
            _hubControl = hubControl;
        }

        public ChannelReader<PositionDto> StreamPosition(string exchange, Instrument instrument)
        {
            return _hubControl.StreamPositions(ExchangeUtils.GetExchangePositionKey(exchange, instrument));
        }
    }
}