using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bognabot.Exchanges.Bitmex.Core;
using Bognabot.Exchanges.Bitmex.Trade;
using Bognabot.Exchanges.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bognabot.Exchanges.Bitmex.Trade
{
    public class BitmexTradeCommand : ICommand
    {
        public IDictionary<string, string> GetRequestParams(CommandRequest request)
        {
            if (!(request is BitmexTradeCommandRequest))
                return null;

            return request.AsDictionary();
        }
    }
}
