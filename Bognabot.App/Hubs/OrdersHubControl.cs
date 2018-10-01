﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Bitmex;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Trader.Enums;
using Bognabot.Services.Exchange;
using Bognabot.Trader;
using Microsoft.AspNetCore.SignalR;

namespace Bognabot.App.Hubs
{
    public class OrdersHubControl
    {
        private readonly IHubContext<OrdersHub> _hub;
        private readonly OrderService _orderService;
        private readonly Dictionary<string, Channel<PositionDto>> _positionChannels;

        public OrdersHubControl(IHubContext<OrdersHub> hub, OrderService orderService)
        {
            _hub = hub;
            _orderService = orderService;
            _positionChannels = new Dictionary<string, Channel<PositionDto>>();
        }

        public ChannelReader<PositionDto> StreamPositions(string positionKey)
        {
            var channel = _positionChannels.ContainsKey(positionKey) 
                ? _positionChannels[positionKey] 
                : Channel.CreateUnbounded<PositionDto>();

            _orderService.StreamPositionAsync(positionKey, async position =>
            {
                await channel.Writer.WriteAsync(position);
            });

            return channel.Reader;
        }
    }
}
