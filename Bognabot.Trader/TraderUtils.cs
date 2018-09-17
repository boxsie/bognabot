using System;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Trader
{
    public static class TraderUtils
    {
        public static bool IsAlmostZero(this double value)
        {
            return Math.Abs(value) < double.Epsilon;
        }

        public static bool AlmostEqual(this double value, double compareTo)
        {
            return Math.Abs(value - compareTo) < double.Epsilon;
        }

        public static void Subtract(this double[] src, double[] dst)
        {
            for (var i = 0; i < src.Length; i++)
                src[i] -= dst[i];
        }

        public static double[] CreateSubstract(this double[] src, double[] dst)
        {
            var t = new double[src.Length];

            for (var i = 0; i < src.Length; i++)
                t[i] = src[i] - dst[i];

            return t;
        }
    }
}
