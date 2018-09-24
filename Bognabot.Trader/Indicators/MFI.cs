using System.Linq;
using Bognabot.Data.Exchange.Dtos;

namespace Bognabot.Trader.Indicators
{
    public class MFI : IIndicator
    {            
        /*
            Typical price = (high price + low price + closing price) / 3
            Raw money flow = typical price x volume
            Money flow ratio = (14-day Positive Money Flow) / (14-day Negative Money Flow)
            Positive money flow is calculated by summing up all of the money flow on the days in the period
            where the typical price is higher than the previous typical price. This same logic applies for the negative money flow.
            MFI = 100 - 100 / (1 + money flow ratio)
        */

        public double[] Calculate(CandleDto[] candles, int period)
        {
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
    }
}