using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Exchange.Enums;

namespace Bognabot.Services.Exchange.Contracts
{
    public interface IExchangeService
    {
        ExchangeConfig ExchangeConfig { get; }
        DateTime Now { get; }

        void ConfigureMap(IMapperConfigurationExpression cfg);
        Task StartAsync();
        Task SubscribeToStreamAsync<T>(ExchangeChannel channel, IStreamSubscription subscription, Instrument? instrument = null) where T : ExchangeDto;

        Task<List<CandleDto>> GetCandlesAsync(Instrument instrument, TimePeriod timePeriod, DateTime startTime, DateTime endTime);
        Task<OrderDto> PlaceOrderAsync(Instrument instrument, double price, double quantity, TradeSide side, OrderType orderType);
    }
}