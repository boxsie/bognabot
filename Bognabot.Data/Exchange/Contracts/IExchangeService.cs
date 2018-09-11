using System;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Models.Exchange;

namespace Bognabot.Data.Exchange.Contracts
{
    public interface IExchangeService
    {
        ExchangeConfig ExchangeConfig { get; }
        DateTimeOffset Now { get; }

        event Func<TradeModel[], Task> OnTradeReceived;
        event Func<BookModel[], Task> OnBookReceived;

        void ConfigureMap(IMapperConfigurationExpression cfg);
        Task SubscribeToStreams();
        Task GetCandlesAsync(Instrument instrument, TimePeriod timePeriod, DateTimeOffset startTime, DateTimeOffset endTime, Func<CandleModel[], Task> onRecieve);
    }
}