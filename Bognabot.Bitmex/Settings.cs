using System;
using AutoMapper;

namespace Bognabot.Bitmex
{
    public static class Settings
    {
        public static string BitmexApiKey => "iiMc1cci1lNhyVpmrILHqCRY";
        public static string BitmexApiSecret => "nkj7IJuPVugn8kYmIqXQ093ho7Z8ccipeNbn6RTZrosnZCUh";
        public static Uri BitmexApiUri => new Uri("wss://testnet.bitmex.com/realtime");
    }

    public static class BitmexAutoMapperConfig
    {
        public static IMapper GetMapper()
        {
            var config = new MapperConfiguration(x =>
            {
            });

            config.AssertConfigurationIsValid();

            return config.CreateMapper();
        }
    }

    public static class MappingExpressionExtensions
    {
        public static IMappingExpression<TSource, TDest> IgnoreAllUnmapped<TSource, TDest>(this IMappingExpression<TSource, TDest> expression)
        {
            expression.ForAllMembers(opt => opt.Ignore());

            return expression;
        }
    }
}