using System;
using System.Threading.Tasks;
using Bognabot.Data.Models.Exchange;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Data.Exchange.Contracts
{
    public interface IExchangeService
    {
        ExchangeType ExchangeType { get; }

        DateTimeOffset Now { get; }

        event Func<TradeModel[], Task> OnTradeReceived;
        event Func<BookModel[], Task> OnBookReceived;

        Task SubscribeToStreams();
        Task GetCandlesAsync(TimePeriod candleSize, DateTimeOffset startTime, DateTimeOffset endTime, Func<CandleModel[], Task> onRecieve);
    }
}