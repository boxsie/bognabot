using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Exchange.Models;
using NLog;

namespace Bognabot.Services.Exchange
{
    public interface IExchangeService
    {
        ExchangeConfig ExchangeConfig { get; }
        DateTimeOffset Now { get; }
        
        void ConfigureMap(IMapperConfigurationExpression cfg);

        Task StartAsync();
        Task SubscribeToStreamAsync<T>(ExchangeChannel channel, IStreamSubscription subscription) where T : ExchangeModel;
        Task<List<CandleModel>> GetCandlesAsync(Instrument instrument, TimePeriod timePeriod, DateTimeOffset startTime, DateTimeOffset endTime);
    }
}