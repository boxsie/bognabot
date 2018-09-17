using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Trader.Signals
{
    public class OverBoughtOverSoldSignal : ISignal
    {
        public List<TimePeriod> SupportedTimePeriods => new List<TimePeriod>
        {
            TimePeriod.OneMinute,
            TimePeriod.FiveMinutes,
            TimePeriod.FifteenMinutes,
            TimePeriod.OneHour,
            TimePeriod.OneDay
        };

        public Task<SignalStrength> ProcessSignal(TimePeriod timePeriod, Candle[] candles)
        {
            var cmo = Indicators.CMO(9, candles);
            var cci = Indicators.CCI(20, candles);
            var mfi = Indicators.MFI(14, candles);
            
            var cmoUnit = Normalise(cmo.First(), -50, 50);
            var cciUnit = Normalise(cci.First(), -100, 100);
            var mfiUnit = Normalise(mfi.First(), 10, 80);

            var avg = (cmoUnit + cciUnit + mfiUnit) / 3;

            var strengthCount = Enum.GetNames(typeof(SignalStrength));

            var ss = (SignalStrength)(int)(strengthCount.Length - (strengthCount.Length * avg));

            return Task.FromResult(ss);
        }

        private double Normalise(double val, double min, double max)
        {
            return Math.Clamp((val - min) / (max - min), 0, 1);
        }
    }
}