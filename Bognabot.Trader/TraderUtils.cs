using System;
using System.Linq;
using Bognabot.Data.Trader.Enums;
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

        public static double NormaliseAndClamp(double val, double min, double max)
        {
            return Math.Clamp((val - min) / (max - min), 0, 1);
        }


        /// <summary>
        /// Takes a value from 0 to 1 and converts it to a value between 0 and N
        /// where 0 is the strongest sell signal and N is the strongest buy signal
        /// </summary>
        /// <param name="strength">The stregth of the signal from 0 (weakest) to 1 (strongest)</param>
        /// <returns></returns>
        public static SignalStrength ToSignalStrength(double strength)
        {
            var strengthCount = Enum.GetNames(typeof(SignalStrength));
            return (SignalStrength)(int)(strengthCount.Length - (strengthCount.Length * strength));
        }
    }
}
