using System;
using System.Linq;
using Bognabot.Data.Exchange.Dtos;

namespace Bognabot.Trader.Indicators
{
    public class CCI : IIndicator
    {
        /*
         CCI = (Typical Price - n-period SMA of TP) / (.015 x Mean Deviation)

         Typical Price (TP) = (High + Low + Close)/3
         Constant = .015  

         There are four steps to calculating the Mean Deviation: 
         First, subtract the most recent n-period average of the typical price from each period's typical price. 
         Second, take the absolute values of these numbers. 
         Third, sum the absolute values. 
         Fourth, divide by the total number of periods.                        
       */

        public double[] Calculate(CandleDto[] candles, int period)
        {


            var cci = new double[candles.Length - (period - 1)];

            for (var i = 0; i < cci.Length; i++)
            {
                var typicalPrices = candles
                    .Skip(i)
                    .Take(period)
                    .Select(c => (c.High + c.Low + c.Close) / 3)
                    .ToArray();

                var tpAverage = typicalPrices.Average();
                var meanDeviation = typicalPrices.Select(tp => Math.Abs(tp - tpAverage)).Sum() / period;

                cci[i] = (typicalPrices.First() - typicalPrices.Average()) / (0.015d * meanDeviation);
            }

            return cci;
        }
    }
}