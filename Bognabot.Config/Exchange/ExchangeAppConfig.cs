using System;
using System.Collections.Generic;
using Bognabot.Config.Core;

namespace Bognabot.Config.Exchange
{
    public class ExchangeAppConfig : Core.AppConfig
    {
        public ExchangeSpecificAppConfig Bitmex { get; set; }
    }
}