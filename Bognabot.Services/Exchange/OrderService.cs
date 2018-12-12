using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Trader.Models;
using Bognabot.Services.Exchange.Contracts;
using NLog;

namespace Bognabot.Services.Exchange
{
    public class OrderService
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<IExchangeService> _exchangeServices;
        private readonly Dictionary<string, Dictionary<Instrument, PositionDto>> _exchangePositions;
        private readonly Dictionary<string, IStreamSubscription> _exchangePositionSubscriptions;
        private readonly Dictionary<string, List<Func<PositionDto, Task>>> _positionCallbacks; 

        public OrderService(ILogger logger, IEnumerable<IExchangeService> exchangeServices)
        {
            _logger = logger;
            _exchangeServices = exchangeServices;

            _exchangePositions = new Dictionary<string, Dictionary<Instrument, PositionDto>>();
            _exchangePositionSubscriptions = new Dictionary<string, IStreamSubscription>();
            _positionCallbacks = new Dictionary<string, List<Func<PositionDto, Task>>>();

            foreach (var exchangeService in _exchangeServices)
            {
                var name = exchangeService.ExchangeConfig.ExchangeName;

                _exchangePositions.Add(name, new Dictionary<Instrument, PositionDto>());
            }
        }

        public async Task StartAsync()
        {
            foreach (var exchangeService in _exchangeServices)
            {
                var sub = new StreamSubscription<PositionDto>(OnPositionUpdate);

                _exchangePositionSubscriptions.Add(exchangeService.ExchangeConfig.ExchangeName, sub);

                await exchangeService.SubscribeToStreamAsync<PositionDto>(ExchangeChannel.Position, sub);
            }
        }

        public async Task<OrderDto> PlaceOrderAsync(OrderModel orderModel)
        {
            var exchange = _exchangeServices.FirstOrDefault(x => x.ExchangeConfig.ExchangeName == orderModel.Exchange);

            if (exchange == null)
            { 
                _logger.Log(LogLevel.Error, $"Order cannot be placed as {orderModel.Exchange} exchange cannot be found");
                return null;
            }

            var order = await exchange.PlaceOrderAsync(orderModel.Instrument, orderModel.Price, orderModel.Amount, orderModel.Side, orderModel.OrderType);

            if (orderModel.OrderType == OrderType.Market)
            {
                if (orderModel.OrderProfitAmount > 0)
                    await exchange.PlaceOrderAsync(orderModel.Instrument, orderModel.Price, orderModel.OrderProfitAmount,
                        orderModel.Side == TradeSide.Buy ? TradeSide.Buy : TradeSide.Sell, OrderType.Limit);

                if (orderModel.OrderStopAmount > 0)
                    await exchange.PlaceOrderAsync(orderModel.Instrument, orderModel.Price, orderModel.OrderStopAmount,
                        orderModel.Side == TradeSide.Buy ? TradeSide.Buy : TradeSide.Sell, OrderType.Stop);
            }

            return order;
        }

        public void StreamPosition(string positionKey, Func<PositionDto, Task> callback)
        {
            if (!_positionCallbacks.ContainsKey(positionKey))
                _positionCallbacks[positionKey] = new List<Func<PositionDto, Task>>();

            _positionCallbacks[positionKey].Add(callback);
        }

        public List<PositionDto> GetAllPositions()
        {
            return _exchangePositions.Values.SelectMany(x => x.Values).ToList();
        }

        private async Task OnPositionUpdate(PositionDto[] positions)
        {
            if (positions == null || !positions.Any())
                return;

            var exchangeName = positions.First().ExchangeName;

            if (!_exchangePositions.ContainsKey(exchangeName))
                return;

            var exchangePosition = _exchangePositions[exchangeName];

            foreach (var position in positions)
            {
                if (exchangePosition.ContainsKey(position.Instrument))
                    exchangePosition[position.Instrument] = Mapper.Map(position, exchangePosition[position.Instrument]);
                else
                    exchangePosition.Add(position.Instrument, position);

                var key = ExchangeUtils.GetExchangePositionKey(exchangeName, position.Instrument);

                if (!_positionCallbacks.ContainsKey(key))
                    continue;

                var callbacks = _positionCallbacks[key];

                foreach (var callback in callbacks)
                    await callback.Invoke(exchangePosition[position.Instrument]);
            }
        }
    }
}