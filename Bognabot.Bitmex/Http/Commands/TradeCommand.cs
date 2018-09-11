using System.Collections.Generic;
using Bognabot.Bitmex.Core;
using Bognabot.Bitmex.Http.Requests;
using Bognabot.Data.Exchange;
using Bognabot.Net;

namespace Bognabot.Bitmex.Http.Commands
{
    public class TradeCommand : IHttpCommand
    {
        public IDictionary<string, string> GetRequestParams(CommandRequest request)
        {
            if (!(request is TradeCommandRequest))
                return null;

            return request.AsDictionary();
        }
    }
}
