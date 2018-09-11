using System.Collections.Generic;

namespace Bognabot.Data.Exchange
{
    public interface IHttpCommand
    {
        IDictionary<string, string> GetRequestParams(CommandRequest request);
    }
}