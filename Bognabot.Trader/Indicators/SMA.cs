using System.Linq;
using Bognabot.Data.Exchange.Dtos;

namespace Bognabot.Trader.Indicators
{
    public class SMA : IIndicator
    {
        /*
         * A simple moving average is formed by computing the average price of a
         * security over a specific number of periods.
         */

        public double[] Calculate(CandleDto[] candles, int period)
        {
            var sma = new double[candles.Length - (period - 1)];
            
            for (var i = 0; i < sma.Length; i++)
                sma[i] = candles.Skip(i).Take(period).Select(x => x.Close).Average();

            return sma;
        }
    }
}