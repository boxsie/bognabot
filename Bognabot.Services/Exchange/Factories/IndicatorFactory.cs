using System;
using System.Collections.Generic;
using System.Linq;
using Bognabot.Trader;
using NLog;

namespace Bognabot.Services.Exchange.Factories
{
    public class IndicatorFactory
    {
        private readonly ILogger _logger;
        private readonly Dictionary<Type, IIndicator> _indicators;

        public IndicatorFactory(ILogger logger, IEnumerable<IIndicator> indicators)
        {
            _logger = logger;
            _indicators = indicators.ToDictionary(x => x.GetType());
        }

        public T Get<T>() where T : IIndicator
        {
            var tType = typeof(T);

            if (_indicators.ContainsKey(tType))
                return (T) _indicators[tType];

            _logger.Log(LogLevel.Error, $"{tType} is not a registered indicator");
            return default(T);
        }
    }
}