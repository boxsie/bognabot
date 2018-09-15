using System;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Trader
{
    public static class Indicators
    {
        /// <summary>
        /// Accelerator/Decelerator oscillator
        /// </summary>
        public static double[] AC(double[] prices, int period)
        {
            var ao = AO(prices);
            var smaOfAo = SMA(ao, 5);

            var ac = new double[prices.Length];

            for (var i = 0; i < prices.Length; ++i)
                ac[i] = ao[i] - smaOfAo[i];

            return ac;
        }

        /// <summary>
        /// Average Directional Movement Index
        /// </summary>
        public static double[] ADX(double[] prices, int period, Candle[] candles)
        {
            var dx = new double[prices.Length];
            var pDi = DmiPlus(prices, period, candles);
            var mDi = DmiMinus(prices, period, candles);

            for (var i = 0; i < prices.Length; ++i)
            {
                var diff = pDi[i] + mDi[i];
                if (diff.IsAlmostZero())
                {
                    dx[i] = 0;
                }
                else
                {
                    dx[i] = 100 * (Math.Abs(pDi[i] - mDi[i]) / (pDi[i] + mDi[i]));
                }
            }

            var adx = EMA(dx, period);

            return adx;
        }

        /// <summary>
        /// Awesome Oscillator
        /// </summary>
        public static double[] AO(double[] prices)
        {
            var fastSma = SMA(prices, 5);
            var slowSma = SMA(prices, 34);
            var ao = new double[prices.Length];

            for (var i = 0; i < prices.Length; i++)
                ao[i] = fastSma[i] - slowSma[i];

            return ao;
        }

        /// <summary>
        /// Average True Range
        /// </summary>
        public static double[] ATR(int period, Candle[] candles)
        {
            var temp = new double[candles.Length];

            temp[0] = 0d;

            for (var i = 1; i < candles.Length; i++)
            {
                var diff1 = Math.Abs(candles[i - 1].Close - candles[i].High);
                var diff2 = Math.Abs(candles[i - 1].Close - candles[i].Low);
                var diff3 = candles[i].High - candles[i].Low;

                var max = diff1 > diff2 ? diff1 : diff2;
                temp[i] = max > diff3 ? max : diff3;
            }

            var atr = SMA(temp, period);

            return atr;
        }

        /// <summary>
        /// Average True Range Percentage
        /// </summary>
        public static double[] ATRP(int period, Candle[] candles)
        {
            var atr = ATR(period, candles);
            var atrp = new double[atr.Length];

            for (var i = period; i < candles.Length; i++)
                atrp[i] = atr[i] * 100.0 / candles[i].Close;

            return atrp;
        }

        /// <summary>
        /// Bears Power
        /// </summary>
        public static double[] BearsPower(double[] prices, int period, Candle[] candles)
        {
            var bears = new double[prices.Length];

            var ema = EMA(prices, period);

            for (var i = 0; i < prices.Length; i++)
                bears[i] = candles[i].Low - ema[i];

            return bears;
        }

        /// <summary>
        /// Balance Of Power
        /// </summary>
        public static double[] BOP(Candle[] candles)
        {
            var bop = new double[candles.Length];

            for (var i = 0; i < candles.Length; i++)
            {
                if (candles[i].High.AlmostEqual(candles[i].Low))
                    bop[i] = 0;
                else
                    bop[i] = (candles[i].Close - candles[i].Open) / (candles[i].High - candles[i].Low);
            }

            return bop;
        }

        /// <summary>
        /// Bulls Power
        /// </summary>
        public static double[] BullsPower(double[] prices, int period, Candle[] candles)
        {
            var bulls = new double[prices.Length];

            var ema = EMA(prices, period);

            for (var i = 0; i < prices.Length; i++)
                bulls[i] = candles[i].High - ema[i];

            return bulls;
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
        /// Directional Movement Index Minus
        /// </summary>
        public static double[] DmiMinus(double[] prices, int period, Candle[] candles)
        {
            var pdm = new double[prices.Length];

            pdm[0] = 0d;

            for (int i = 1; i < prices.Length; ++i)
            {
                var plusDm = candles[i].High - candles[i - 1].High;
                var minusDm = candles[i - 1].Low - candles[i].Low;

                if (plusDm < 0)
                    plusDm = 0;

                if (minusDm < 0)
                    minusDm = 0;

                if (plusDm.AlmostEqual(minusDm))
                    plusDm = 0;
                else if (plusDm < minusDm)
                    plusDm = 0;

                var trueHigh = candles[i].High > prices[i - 1] ? candles[i].High : prices[i - 1];
                var trueLow = candles[i].Low < prices[i - 1] ? candles[i].Low : prices[i - 1];
                var tr = trueHigh - trueLow;

                if (tr.IsAlmostZero())
                    pdm[i] = 0;
                else
                    pdm[i] = 100 * plusDm / tr;
            }

            var dmi = EMA(pdm, period);

            return dmi;
        }

        /// <summary>
        /// Directional Movement Index Plus
        /// </summary>
        public static double[] DmiPlus(double[] prices, int period, Candle[] candles)
        {
            var mdm = new double[prices.Length];

            mdm[0] = 0d;

            for (var i = 1; i < prices.Length; ++i)
            {
                var plusDm = candles[i].High - candles[i - 1].High;
                var minusDm = candles[i - 1].Low - candles[i].Low;

                if (plusDm < 0)
                    plusDm = 0;

                if (minusDm < 0)
                    minusDm = 0;

                if (plusDm.AlmostEqual(minusDm))
                    minusDm = 0;
                else if (plusDm >= minusDm)
                    minusDm = 0;

                var trueHigh = candles[i].High > prices[i - 1] ? candles[i].High : prices[i - 1];
                var trueLow = candles[i].Low < prices[i - 1] ? candles[i].Low : prices[i - 1];

                var tr = trueHigh - trueLow;

                if (tr.IsAlmostZero())
                    mdm[i] = 0;
                else
                    mdm[i] = 100 * minusDm / tr;
            }

            var dmi = EMA(mdm, period);

            return dmi;
        }

        /// <summary>
        /// Exponential Moving Average
        /// </summary>
        public static double[] EMA(double[] prices, int period)
        {
            var ema = new double[prices.Length];
            var sum = prices[0];
            var coeff = 2.0 / (1.0 + period);

            for (var i = 0; i < prices.Length; i++)
            {
                sum += coeff * (prices[i] - sum);
                ema[i] = sum;
            }

            return ema;
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
        /// Simple Moving Average
        /// </summary>
        public static double[] SMA(double[] prices, int period)
        {
            var sma = new double[prices.Length];
            var sum = 0d;

            for (var i = 0; i < period; i++)
            {
                sum += prices[i];
                sma[i] = sum / (i + 1);
            }

            for (var i = period; i < prices.Length; i++)
            {
                sum = 0;

                for (var j = i; j > i - period; j--)
                    sum += prices[j];

                sma[i] = sum / period;
            }

            return sma;
        }

        /// <summary>
        /// Triple-smoothed Exponential Moving Average
        /// </summary>
        public static double[] TRIX(double[] prices, int period)
        {
            var trix = new double[prices.Length];
            var ema1 = EMA(prices, period);
            var ema2 = EMA(ema1, period);
            var ema3 = EMA(ema2, period);

            trix[0] = 0.0;

            for (var i = 1; i < prices.Length; ++i)
            {
                if (ema3[i].IsAlmostZero())
                    trix[i] = 0.0;
                else
                    trix[i] = 100.0 * ((ema3[i] - ema3[i - 1]) / ema3[i]);
            }

            return trix;
        }

        /// <summary>
        /// Williams Percent Range
        /// </summary>
        public static double[] WPR(double[] prices, int period, Candle[] candles)
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
    }
}
