using System;
using Bognabot.Data.Exchange.Dtos;

namespace Bognabot.Trader.Indicators
{
    public class ADX : IIndicator
    {
        public double[] Calculate(CandleDto[] candles, int period)
        {
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

            var atr = TraderUtils.WildersSmoothing(new ATR().Calculate(candles, period));
            pdi = TraderUtils.WildersSmoothing(pdi);
            mdi = TraderUtils.WildersSmoothing(mdi);

            for (var i = 0; i < adx.Length; i++)
            {
                var tr = atr[i];
                var p = (pdi[i] / tr) * 100;
                var m = (mdi[i] / tr) * 100;

                adx[i] = 100 * (Math.Abs(p - m) / (p + m));
            }

            return TraderUtils.WildersSmoothing(adx);
        }
    }
}