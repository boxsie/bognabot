using System.Collections.Generic;
using System.Threading.Tasks;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Trader.Enums;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Trader
{
    public interface ISignal
    {
        List<TimePeriod> SupportedTimePeriods { get; }
        Task<SignalStrength> ProcessSignal(TimePeriod timePeriod, Candle[] candles);
    }
}