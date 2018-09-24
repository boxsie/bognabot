using System;
using System.Linq;
using Bognabot.Data.Exchange.Dtos;

namespace Bognabot.Trader.Indicators
{
    public class ATR : IIndicator
    {
        /*
            To calculate the ATR, the True Range first needs to be discovered. True Range takes into account 
            the most current period high/low range as well as the previous period close if necessary. 

            There are three calculation which need to be completed and then compared against each other.

            The True Range is the largest of the following:

            The Current Period High minus (-) Current Period Low
            The Absolute Value (abs) of the Current Period High minus (-) The Previous Period Close
            The Absolute Value (abs) of the Current Period Low minus (-) The Previous Period Close

            true range=max[(high - low), abs(high - previous close), abs (low - previous close)]

            *Absolute Value is used because the ATR does not measure price direction, only volatility. 
            Therefore there should be no negative numbers.

            *Once you have the True Range, the Average True Range can be plotted. 
            The ATR is an Exponential Moving Average of the True Range.           
        */

        public double[] Calculate(CandleDto[] candles, int period)
        {
            var atr = new double[candles.Length - 1];

            for (var i = 0; i < atr.Length - 1; i++)
            {
                var current = candles[i];
                var prev = candles[i + 1];

                var ranges = new[]
                {
                    current.High - current.Low,
                    Math.Abs(current.High - prev.Close),
                    Math.Abs(current.Low - prev.Close)
                };

                atr[i] = ranges.Max();
            }

            return atr;
        }
    }
}