using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Exchange.Enums;
using NLog;

namespace Bognabot.Services.Exchange
{
    public interface IExchangeService
    {
        ExchangeConfig ExchangeConfig { get; }
        DateTime Now { get; }
        
        void ConfigureMap(IMapperConfigurationExpression cfg);

        Task StartAsync();
        Task SubscribeToStreamAsync<T>(ExchangeChannel channel, Instrument instrument, IStreamSubscription subscription) where T : ExchangeDto;
        Task<List<CandleDto>> GetCandlesAsync(Instrument instrument, TimePeriod timePeriod, DateTime startTime, DateTime endTime);
    }
}