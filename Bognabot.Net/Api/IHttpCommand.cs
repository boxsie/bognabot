using System.Collections.Generic;

namespace Bognabot.Net.Api
{
    public interface IHttpCommand
    {
        IDictionary<string, string> GetRequestParams(CommandRequest request);
    }
}