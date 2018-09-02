using System.Collections.Generic;
using Bognabot.Bitmex.Core;
using Bognabot.Bitmex.Http.Requests;
using Bognabot.Net.Api;

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
