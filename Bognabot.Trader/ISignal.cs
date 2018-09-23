using System.Collections.Generic;
using System.Threading.Tasks;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Trader.Enums;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Trader
{
    public interface IIndicator
    {
        double[] Calculate(CandleDto[] candles, int period);
    }

    public interface ISignal
    {
        Task<bool> IsPeriodSupportedAsync(TimePeriod period);
        Task<SignalStrength> ProcessSignalAsync(TimePeriod timePeriod, CandleDto[] candles);
    }
}