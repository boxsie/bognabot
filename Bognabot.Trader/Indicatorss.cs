using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Trader
{
    public static class Indicatorss
    {
        /// <summary>
        /// Average Directional Movement Index
        /// </summary>
        public static double[] ADX(CandleDto[] candles, int period)
        {
            if (candles.Length < period + 1)
                throw new IndexOutOfRangeException("The ATR indicator requires period + 1 candles");

            var adx = new double[period];
            var pdi = new double[period];
            var mdi = new double[period];

            for (var i = 0; i < adx.Length; i++)
            {
                var current = candles[i];
                var prev = candles[i + 1];

                var p = current.High - prev.High;
                var m = prev.Low - current.Low;

                if (p.AlmostEqual(m)) p = m = 0;
                if (p < 0 || m > p) p = 0;
                if (m < 0 || p > m) m = 0;

                pdi[i] = p; 
                mdi[i] = m;
            }

            var atr = WildersSmoothing(ATR(candles));
            pdi = WildersSmoothing(pdi);
            mdi = WildersSmoothing(mdi);

            for (var i = 0; i < adx.Length; i++)
            {
                var tr = atr[i];
                var p = (pdi[i] / tr) * 100;
                var m = (mdi[i] / tr) * 100;

                adx[i] = 100 * (Math.Abs(p - m) / (p + m));
            }

            return WildersSmoothing(adx);
        }

        /// <summary>
        /// Average True Range
        /// </summary>
        public static double[] ATR(CandleDto[] candles)
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

        /// <summary>
        /// Average True Range Percentage
        /// </summary>
        public static double[] ATRP(CandleDto[] candles, int period)
        {
            var atr = ATR(candles);
            var atrp = new double[atr.Length];

            for (var i = period; i < candles.Length; i++)
                atrp[i] = atr[i] * 100.0 / candles[i].Close;

            return atrp;
        }
        
        /// <summary>
        /// Center of Gravity oscillator
        /// </summary>
        public static double[] COG(double[] prices, int period)
        {
            var cog = new double[prices.Length];

            for (var i = period - 1; i < prices.Length; ++i)
            {
                var weightedSum = 0d;
                var sum = 0d;

                for (var j = 0; j < period; ++j)
                {
                    weightedSum += prices[i - period + j + 1] * (period - j);
                    sum += prices[i - period + j + 1];
                }

                cog[i] = -weightedSum / sum;
            }

            return cog;
        }

        /// <summary>
        /// Commodity Channel Index
        /// </summary>
        public static double[] CCI(CandleDto[] candles, int period)
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

        /// <summary>
        /// Chande Momentum Oscillator
        /// </summary>
        public static double[] CMO(CandleDto[] candles, int period)
        {
            if (candles.Length < period + 1)
                throw new IndexOutOfRangeException("The CMO indicator requires period + 1 candles");

            /*
                CMO = ((SU - SD) / (SU + SD)) * 100

                Calculate the difference between closing price for the current and the previous period.
                If the change is positive, add it to the sum of up days (SU) for the specified period.
                If the change is negative, add the absolute value to the sum of down days (SD) for the specified period.
                To calculate Chande Momentum for the specified period (normally 20), take the difference, SU - SD, and divide by total movement, SU + SD.
            */

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

        /// <summary>
        /// Exponential Moving Average
        /// </summary>
        public static double[] EMA(double[] values)
        {
            var ema = new double[values.Length];
            var k = 2 / (ema.Length + 1);
            
            for (var i = 0; i < ema.Length; i++)
            {
                var prevEma = i == ema.Length - 1 
                    ? values.Average() 
                    : ema[i - 1];

                ema[i] = prevEma * (ema.Length - 1) + values[i] * k + prevEma * (1 - k);
            }

            return ema.Reverse().ToArray();
        }

        /// <summary>
        /// Exponential Moving Average
        /// </summary>
        public static double[] EMA(double[] values, int period)
        {
            var ema = new double[values.Length];
            var k = 2 / (ema.Length + 1);

            for (var i = 0; i < ema.Length; i++)
            {
                var prevEma = i == ema.Length - 1
                    ? values.Average()
                    : ema[i - 1];

                ema[i] = prevEma * (ema.Length - 1) + values[i] * k + prevEma * (1 - k);
            }

            return ema.Reverse().ToArray();
        }

        /// <summary>
        /// Highest Value
        /// </summary>
        public static double[] Highest(double[] prices, int period)
        {
            var highest = new double[prices.Length];

            highest[0] = prices[0];

            for (var i = 1; i < period; ++i)
            {
                if (prices[i] > highest[i - 1])
                    highest[i] = prices[i];
                else
                    highest[i] = highest[i - 1];
            }

            var highestIdx = 0;

            for (var i = period; i < prices.Length; ++i)
            {
                var highestHigh = double.MinValue;
                var start = Math.Max(i - period + 1, highestIdx);

                for (var s = start; s <= i; ++s)
                {
                    if (prices[s] > highestHigh)
                    {
                        highestHigh = prices[s];
                        highestIdx = s;
                    }
                }

                highest[i] = highestHigh;
            }

            return highest;
        }

        /// <summary>
        /// Lowest Value
        /// </summary>
        public static double[] Lowest(double[] prices, int period)
        {
            var lowest = new double[prices.Length];

            lowest[0] = prices[0];

            for (var i = 1; i < period; ++i)
            {
                if (prices[i] < lowest[i - 1])
                    lowest[i] = prices[i];
                else
                    lowest[i] = lowest[i - 1];
            }

            var lowestIdx = 0;

            for (var i = period; i < prices.Length; ++i)
            {
                var lowestLow = double.MaxValue;
                var start = Math.Max(i - period + 1, lowestIdx);

                for (var s = start; s <= i; ++s)
                {
                    if (prices[s] < lowestLow)
                    {
                        lowestLow = prices[s];
                        lowestIdx = s;
                    }
                }

                lowest[i] = lowestLow;
            }

            return lowest;
        }

        /// <summary>
        /// Linearly Weighted Moving Average
        /// </summary>
        public static double[] LWMA(double[] prices, int period)
        {
            var lwma = new double[prices.Length];
            var avgsum = 0d;
            var sum = 0d;

            for (var i = 0; i < period - 1; i++)
            {
                avgsum += prices[i] * (i + 1);
                sum += prices[i];
            }

            var divider = period * (period + 1) / 2;

            for (var i = period - 1; i < prices.Length; i++)
            {
                avgsum += prices[i] * period;
                sum += prices[i];
                lwma[i] = avgsum / divider;
                avgsum -= sum;
                sum -= prices[i - period + 1];
            }

            return lwma;
        }

        /// <summary>
        /// Money Flow Index
        /// </summary>
        public static double[] MFI(CandleDto[] candles, int period)
        {
            if (candles.Length < period + 1)
                throw new IndexOutOfRangeException("The MFI indicator requires period + 1 candles");

            /*
                Typical price = (high price + low price + closing price) / 3
                Raw money flow = typical price x volume
                Money flow ratio = (14-day Positive Money Flow) / (14-day Negative Money Flow)
                Positive money flow is calculated by summing up all of the money flow on the days in the period
                where the typical price is higher than the previous typical price. This same logic applies for the negative money flow.
                MFI = 100 - 100 / (1 + money flow ratio)
            */

            var mfi = new double[candles.Length - (period - 1)];

            for (var i = 0; i < mfi.Length; i++)
            {
                var rawMoneyFlow = candles
                    .Skip(i)
                    .Take(period + 1)
                    .Select(c => ((c.High + c.Low + c.Close) / 3) * c.Volume)
                    .ToArray();

                if (rawMoneyFlow.Length < period + 1)
                    break;
                
                var posMf = 0d;
                var negMf = 0d;

                for (var o = 0; o < rawMoneyFlow.Length - 1; o++)
                {
                    var rmf = rawMoneyFlow[o];
                    var rmfPrev = rawMoneyFlow[o + 1];

                    if (rmf >= rmfPrev)
                        posMf += rmf;

                    if (rmf <= rmfPrev)
                        negMf += rmf;
                }

                var mfr = posMf / negMf;

                mfi[i] = 100 - (100 / (1 + mfr));
            }

            return mfi;
        }

        /// <summary>
        /// Momentum
        /// </summary>
        public static double[] Momentum(double[] prices, int period)
        {
            var momentum = new double[prices.Length];

            for (var i = 0; i < period; i++)
                momentum[i] = 0;

            for (var i = period; i < prices.Length; i++)
                momentum[i] = prices[i] * 100 / prices[i - period];

            return momentum;
        }

        /// <summary>
        /// Relative Strength Index
        /// </summary>
        public static double[] RSI(double[] prices, int period)
        {
            var rsi = new double[prices.Length];
            var gain = 0d;
            var loss = 0d;

            // first RSI value
            rsi[0] = 0.0;
            for (var i = 1; i <= period; ++i)
            {
                var diff = prices[i] - prices[i - 1];

                if (diff >= 0)
                    gain += diff;
                else
                    loss -= diff;
            }

            var avrg = gain / period;
            var avrl = loss / period;
            var rs = gain / loss;

            rsi[period] = 100 - (100 / (1 + rs));

            for (var i = period + 1; i < prices.Length; ++i)
            {
                var diff = prices[i] - prices[i - 1];

                if (diff >= 0)
                {
                    avrg = ((avrg * (period - 1)) + diff) / period;
                    avrl = (avrl * (period - 1)) / period;
                }
                else
                {
                    avrl = ((avrl * (period - 1)) - diff) / period;
                    avrg = (avrg * (period - 1)) / period;
                }

                rs = avrg / avrl;

                rsi[i] = 100 - (100 / (1 + rs));
            }

            return rsi;
        }

        /// <summary>
        /// Williams Percent Range
        /// </summary>
        public static double[] WPR(double[] prices, int period, CandleDto[] candles)
        {
            var wpr = new double[prices.Length];

            for (var i = period; i < prices.Length; i++)
            {
                var highest = double.MinValue;
                var lowest = double.MaxValue;

                for (int j = i - period + 1; j <= i; j++)
                {
                    if (candles[j].High > highest)
                        highest = candles[j].High;

                    if (candles[j].Low < lowest)
                        lowest = candles[j].Low;
                }

                wpr[i] = -100 * (highest - candles[i].Close) / (highest - lowest);
            }

            return wpr;
        }

        /// <summary>
        /// Wilder's Smoothing Techniques
        /// </summary>
        private static double[] WildersSmoothing(double[] vals)
        {
            var wst = new double[vals.Length];

            wst[0] = vals.Average();

            for (var i = 1; i < wst.Length; i++)
            {
                var prev = wst[i - 1];

                wst[i] = prev + (vals[i] - prev) / vals.Length;
            }

            return wst;
        }

        private static double[] CreateIndicatorArray(int valLen, int period)
        {
            return new double[valLen - (period - 1)];
        }
    }
}
