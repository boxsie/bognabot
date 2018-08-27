using System;

namespace Bognabot.Bitmex
{
    public static class BitmexTime
    {
        public static long NowSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static long Expires()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600;
        }
    }
}