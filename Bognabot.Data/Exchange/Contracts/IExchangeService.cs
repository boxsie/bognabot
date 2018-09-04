using System;
using System.Threading.Tasks;
using Bognabot.Config.Enums;
using Bognabot.Data.Models.Exchange;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Data.Exchange.Contracts
{
    public interface IExchangeService
    {
        SupportedExchange Exchange { get; }

        DateTimeOffset Now { get; }

        event Func<TradeModel[], Task> OnTradeReceived;
        event Func<BookModel[], Task> OnBookReceived;

        Task SubscribeToStreams();
        Task GetCandlesAsync(Instrument instrument, TimePeriod timePeriod, DateTimeOffset startTime, DateTimeOffset endTime, Func<CandleModel[], Task> onRecieve);
    }
}