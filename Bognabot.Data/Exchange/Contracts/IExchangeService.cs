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
        Task SubscribeToTradeChannel(Func func);
        Task SubscribeToBookChannel(Action<BookModel[]> onRecieve);
        Task SubscribeToCandleChannel(TimePeriod period, Action<CandleModel[]> onRecieve);
        Task GetCandlesAsync(Instrument instrument, TimePeriod timePeriod, DateTimeOffset startTime, DateTimeOffset endTime, Func<CandleModel[], Task> onRecieve);
    }
}