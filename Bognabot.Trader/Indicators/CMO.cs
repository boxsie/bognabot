using System;
using System.Linq;
using Bognabot.Data.Exchange.Dtos;

namespace Bognabot.Trader.Indicators
{
    public class CMO : IIndicator
    {
        /*
            CMO = ((SU - SD) / (SU + SD)) * 100

            Calculate the difference between closing price for the current and the previous period.
            If the change is positive, add it to the sum of up days (SU) for the specified period.
            If the change is negative, add the absolute value to the sum of down days (SD) for the specified period.
            To calculate Chande Momentum for the specified period (normally 20), take the difference, SU - SD, and divide by total movement, SU + SD.
        */

        public double[] Calculate(CandleDto[] candles, int period)
        {
            var cmo = new double[candles.Length - (period - 1)];

            for (var i = 0; i < cmo.Length; i++)
            {
                var periodCandles = candles
                    .Skip(i)
                    .Take(period)
                    .ToArray();

                var su = 0d;
                var sd = 0d;

                for (var o = 0; o < period - 1; o++)
                {
                    var delta = periodCandles[o].Close - periodCandles[o + 1].Close;

                    if (delta > 0)
                        su += delta;
                    else
                        sd += Math.Abs(delta);
                }

                cmo[i] = ((su - sd) / (su + sd)) * 100;
            }

            return cmo;
        }
    }
}