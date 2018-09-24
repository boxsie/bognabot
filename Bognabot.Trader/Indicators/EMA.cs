using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Trader.Indicators
{
    public class EMA : IIndicator
    {
        /*
         * There are three steps to calculating an exponential moving average (EMA).
         * First, calculate the simple moving average for the initial EMA value.
         * A simple moving average is used as the previous period's EMA in the first calculation.
         * Second, calculate the weighting multiplier.
         * Third, calculate the exponential moving average for each day between the initial EMA value and today,
         * using the price, the multiplier, and the previous period's EMA value.
         *
         * k = 2 ÷ (Period + 1)
         * EMA = ((Current price - Previous EMA) × k) + Previous EMA
         */

        public double[] Calculate(CandleDto[] candles, int period)
        {
            var ema = new double[candles.Length - ((period * 2) - 1)];
            var k = 2f / (period + 1f);

            for (var i = 0; i < ema.Length; i++)
            {
                // Reverse the array so that 0 is the oldest period
                var periodCandles = candles
                    .Skip(i)
                    .Take(period)
                    .Select(x => x.Close)
                    .Reverse()
                    .ToArray();

                var periodEma = new double[period];

                for (var o = 0; o < periodEma.Length; o++)
                {
                    var prevEma = o == 0
                        ? candles.Skip(i + period).Take(period).Select(x => x.Close).Average()
                        : periodEma[o - 1];

                    periodEma[o] = GetEma(k, periodCandles[o], prevEma);
                }

                ema[i] = periodEma.Last();
            }

            return ema;
        }

        private static double GetEma(float k, double current, double previousEma)
        {
            return (current * k) + (previousEma * (1 - k));
        }
    }
}
