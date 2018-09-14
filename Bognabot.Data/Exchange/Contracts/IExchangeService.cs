using System;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Exchange.Models;
using Bognabot.Data.Models.Exchange;

namespace Bognabot.Data.Exchange.Contracts
{
    public interface IExchangeService
    {
        ExchangeConfig ExchangeConfig { get; }
        DateTimeOffset Now { get; }
        
        void ConfigureMap(IMapperConfigurationExpression cfg);
        Task StartStreamingChannels();

        Task SubscribeToTradeSocketAsync(Func<Task, TradeModel[]> onRecieve);
        Task SubscribeToBookSocketAsync(Func<Task, BookModel[]> onRecieve);
        Task SubscribeToCandleSocketAsync(TimePeriod period, Func<Task, CandleModel[]> onRecieve);

        Task GetCandlesAsync(Instrument instrument, TimePeriod timePeriod, DateTimeOffset startTime, DateTimeOffset endTime, Func<CandleModel[], Task> onRecieve);
    }
}