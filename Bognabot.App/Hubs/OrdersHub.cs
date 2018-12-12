using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Trader.Models;
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

        public Task<OrderDto> PlaceOrder(OrderModel orderModel)
        {
            return _hubControl.PlaceOrderAsync(orderModel);
        }

        public ChannelReader<PositionDto> StreamPosition(string exchange, Instrument instrument)
        {
            return _hubControl.StreamPosition(ExchangeUtils.GetExchangePositionKey(exchange, instrument));
        }

        public List<PositionDto> GetAllPositions()
        {
            return _hubControl.GetAllPositions();
        }
    }
}